using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using App.Application.Positions.Commands.UpdatePosition;
using App.Domain.Employees;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace App.Tests.Application.Positions.Commands.UpdatePosition;

public class UpdatePositionHandlerTests
{
    private readonly Mock<IPositionRepository> _repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);

    private UpdatePositionHandler CreateHandler() => new(_repo.Object, _uow.Object);

    [Fact]
    public async Task Handle_Should_Update_And_Return_Ok()
    {
        // Arrange
        var existing = new Position("Old Name", "OLD", requiresLicense: false);
        var id = existing.Id;

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        // If SaveChangesAsync returns Task<int>:
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);
        // If your IUnitOfWork returns Task instead, change to:
        // _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
        //     .Returns(Task.CompletedTask);

        var handler = CreateHandler();

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "New Name",
            Code: "NEW",
            RequiresLicense: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(Unit.Value);

        // Domain entity actually updated:
        existing.Name.Should().Be("New Name");
        existing.Code.Should().Be("NEW");
        existing.RequiresLicense.Should().BeTrue();

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Position_Missing()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync((Position?)null);

        var handler = CreateHandler();

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "Whatever",
            Code: "WHV",
            RequiresLicense: false
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error.Value.Message.Should().Be("Position not found.");

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Return_Concurrency_When_Save_Conflicts()
    {
        // Arrange
        var existing = new Position("Old Name", "OLD", false);
        var id = existing.Id;

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        var handler = CreateHandler();

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "New Name",
            Code: "NEW",
            RequiresLicense: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("concurrency");
        result.Error.Value.Message.Should().Contain("modified by another process");

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Return_Conflict_When_Save_Hits_Unique_Constraint()
    {
        // Arrange
        var existing = new Position("Old Name", "OLD", false);
        var id = existing.Id;

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateException("duplicate", innerException: null));

        var handler = CreateHandler();

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "New Name",
            Code: "NEW",
            RequiresLicense: true
        );

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("conflict");
        result.Error.Value.Message.Should().Contain("already exists");

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Bubble_Unexpected_Exceptions()
    {
        // Arrange
        var existing = new Position("Old Name", "OLD", false);
        var id = existing.Id;

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(existing);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("weird"));

        var handler = CreateHandler();

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "New Name",
            Code: "NEW",
            RequiresLicense: true
        );

        // Act
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*weird*");

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _repo.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }
}
