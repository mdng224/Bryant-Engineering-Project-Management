using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Employees.Commands.AddEmployee;
using App.Domain.Common;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Employees.Commands.AddEmployee;

public class AddEmployeeHandlerTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock = new();
    private readonly Mock<IUnitOfWork> _uowMock = new();

    private AddEmployeeHandler CreateHandler() => new(_repositoryMock.Object, _uowMock.Object);

    private static AddEmployeeCommand CreateValidCommand() =>
        new(
            FirstName:      "John",
            LastName:       "Doe",
            PreferredName:  "Johnny",
            UserId:         null,
            EmploymentType: EmploymentType.FullTime,
            SalaryType:     SalaryType.Salary,
            Department:     DepartmentType.Engineering,
            HireDate:       null,
            CompanyEmail:   "john.doe@example.com",
            WorkLocation:   "Denver Office",
            Line1:          "123 Main St",
            Line2:          "Suite 100",
            City:           "Denver",
            State:          "CO",
            PostalCode:     "80014"
        );

    [Fact]
    public async Task Handle_WhenSuccessful_ShouldReturnOkResultAndCallRepositoryAndUow()
    {
        // Arrange
        var handler = CreateHandler();
        var command = CreateValidCommand();
        
        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        _repositoryMock.Verify(r => r.Add(It.IsAny<App.Domain.Employees.Employee>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdUniqueConstraintViolated_ShouldReturnUserLinkedError()
    {
        // Arrange
        var handler = CreateHandler();
        var command = CreateValidCommand();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintViolationException(
                message: "Unique constraint violation",
                constraintName: "ux_employees_user_id",
                innerException: new Exception()));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("Employee.UserAlreadyLinked");
        result.Error!.Value.Message.Should().Be("The selected user is already linked to another employee.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<App.Domain.Employees.Employee>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenCompanyEmailUniqueConstraintViolated_ShouldReturnEmailTakenError()
    {
        // Arrange
        var handler = CreateHandler();
        var command = CreateValidCommand();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintViolationException(
                message: "Unique constraint violation",
                constraintName: "ux_employees_company_email",
                innerException: new Exception()));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("Employee.CompanyEmailConflict");
        result.Error!.Value.Message.Should().Be("Another active employee already uses this company email address.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<App.Domain.Employees.Employee>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenOtherUniqueConstraintViolated_ShouldReturnGenericUniqueError()
    {
        // Arrange
        var handler = CreateHandler();
        var command = CreateValidCommand();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintViolationException(
                message: "Unique constraint violation",
                constraintName: "ux_employees_some_other_constraint",
                innerException: new Exception()));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("Employee.UniqueConstraintConflict");
        result.Error!.Value.Message.Should().Be("An employee with the same unique values already exists.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<App.Domain.Employees.Employee>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenConstraintNameNullOrEmpty_ShouldReturnGenericUniqueError()
    {
        // Arrange
        var handler = CreateHandler();
        var command = CreateValidCommand();

        _uowMock
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UniqueConstraintViolationException(
                message: "Unique constraint violation",
                constraintName: null,
                innerException: new Exception()));

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("Employee.UniqueConstraintConflict");
        result.Error!.Value.Message.Should().Be("An employee with the same unique values already exists.");

        _repositoryMock.Verify(r => r.Add(It.IsAny<App.Domain.Employees.Employee>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}