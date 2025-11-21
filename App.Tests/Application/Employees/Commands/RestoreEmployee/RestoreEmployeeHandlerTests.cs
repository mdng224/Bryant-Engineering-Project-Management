using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using App.Application.Employees.Commands.RestoreEmployee;
using App.Domain.Employees;
using App.Tests.Domain.Employees;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace App.Tests.Application.Employees.Commands.RestoreEmployee;

public class RestoreEmployeeHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repoMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private RestoreEmployeeHandler CreateHandler() => new(_repoMock.Object, _uowMock.Object);

    private static Employee CreateActiveEmployee(Guid id)
    {
        var employee = EmployeeTestFactory.CreateActive(id); // <- your own helper
        return employee;
    }

    private static Employee CreateDeletedEmployee(Guid id)
    {
        var employee = EmployeeTestFactory.CreateDeleted(id); // <- your own helper
        return employee;
    }

    [Fact]
    public async Task Handle_WhenEmployeeNotFound_ShouldReturnNotFoundFailure()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var command = new RestoreEmployeeCommand(employeeId);

        _repoMock
            .Setup(r => r.GetAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error!.Value.Message.Should().Be("employee not found.");

        _uowMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmployeeAlreadyActive_ShouldReturnOkAndNotPersist()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var command = new RestoreEmployeeCommand(employeeId);

        var employee = CreateActiveEmployee(employeeId); // IsDeleted == false

        _repoMock
            .Setup(r => r.GetAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _uowMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenEmployeeDeletedAndRestoreSucceeds_ShouldPersistAndReturnOk()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var command = new RestoreEmployeeCommand(employeeId);

        var employee = CreateDeletedEmployee(employeeId); // IsDeleted == true, Restore() => true

        _repoMock
            .Setup(r => r.GetAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _uowMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        _uowMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenDbUpdateExceptionThrown_ShouldReturnConflictFailure()
    {
        // Arrange
        var handler = CreateHandler();
        var employeeId = Guid.NewGuid();
        var command = new RestoreEmployeeCommand(employeeId);

        var employee = CreateDeletedEmployee(employeeId); // IsDeleted == true, Restore() => true

        _repoMock
            .Setup(r => r.GetAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        _uowMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException());

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("conflict");
        result.Error!.Value.Message.Should().Be("Restoring this employee conflicts with an existing active employee.");

        _uowMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
