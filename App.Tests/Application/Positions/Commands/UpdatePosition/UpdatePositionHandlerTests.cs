using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Commands.UpdatePosition;
using App.Domain.Employees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Commands.UpdatePosition;

public class UpdatePositionHandlerTests
{
    [Fact]
    public async Task Handle_Should_Update_And_Return_PositionDto()
    {
        // Arrange
        var writer = new Mock<IPositionWriter>();

        // Existing domain entity (tracked)
        var existing = new Position("Old Name", "OLD", requiresLicense: false);
        var id = existing.Id; // Use the generated id for the command

        writer.Setup(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        writer.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
              .ReturnsAsync(1);

        var handler = new UpdatePositionHandler(writer.Object);

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
        result.Value.Should().NotBeNull();

        var positionDto = result.Value!;
        positionDto.Id.Should().Be(id);
        positionDto.Name.Should().Be("New Name");
        positionDto.Code.Should().Be("NEW"); // adjust if your domain normalizes differently
        positionDto.RequiresLicense.Should().BeTrue();

        writer.Verify(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        writer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Return_NotFound_When_Position_Does_Not_Exist()
    {
        // Arrange
        var writer = new Mock<IPositionWriter>();
        var id = Guid.NewGuid();

        writer.Setup(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
              .ReturnsAsync((Position?)null);

        var handler = new UpdatePositionHandler(writer.Object);

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
        result.Error.Should().NotBeNull();
        result.Error!.Value.Code.Should().Be("not_found");

        writer.Verify(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        writer.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_Should_Bubble_Exception_From_SaveChanges()
    {
        // Arrange
        var writer = new Mock<IPositionWriter>();
        var existing = new Position("Old Name", "OLD", requiresLicense: false);
        var id = existing.Id;

        writer.Setup(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
              .ReturnsAsync(existing);

        writer.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
              .ThrowsAsync(new InvalidOperationException("db error"));

        var handler = new UpdatePositionHandler(writer.Object);

        var command = new UpdatePositionCommand(
            PositionId: id,
            Name: "New Name",
            Code: "NEW",
            RequiresLicense: true
        );

        // Act
        Func<Task> act = async () => await handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*db error*");

        writer.Verify(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        writer.VerifyNoOtherCalls();
    }
}