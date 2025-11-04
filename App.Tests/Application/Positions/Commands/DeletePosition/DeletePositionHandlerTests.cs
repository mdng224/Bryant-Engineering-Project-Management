using App.Application.Abstractions.Persistence;
using App.Application.Positions.Commands.DeletePosition;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Commands.DeletePosition;

public class DeletePositionHandlerTests
{
     [Fact]
        public async Task Handle_Should_Return_Ok_When_Delete_Succeeds()
        {
            // Arrange
            var writer = new Mock<IPositionWriter>();
            var id = Guid.NewGuid();

            writer.Setup(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(true);

            var handler = new DeletePositionHandler(writer.Object);
            var command = new DeletePositionCommand(id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Value.Should().NotBeNull();

            writer.Verify(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            writer.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_Should_Return_NotFound_When_Delete_Fails()
        {
            // Arrange
            var writer = new Mock<IPositionWriter>();
            var id = Guid.NewGuid();

            writer.Setup(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(false);

            var handler = new DeletePositionHandler(writer.Object);
            var command = new DeletePositionCommand(id);

            // Act
            var result = await handler.Handle(command, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeFalse();
            result.Error.Should().NotBeNull();
            result.Error!.Value.Code.Should().Be("not_found");

            writer.Verify(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
            writer.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task Handle_Should_Bubble_Exception_From_DeleteAsync()
        {
            // Arrange
            var writer = new Mock<IPositionWriter>();
            var id = Guid.NewGuid();

            writer.Setup(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new InvalidOperationException("boom"));

            var handler = new DeletePositionHandler(writer.Object);
            var command = new DeletePositionCommand(id);

            // Act
            var act = async () => await handler.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                     .WithMessage("*boom*");

            writer.Verify(w => w.SoftDeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        }
}