using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Writers;
using App.Application.Positions.Commands.AddPosition;
using App.Domain.Employees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Commands.AddPosition;

public class AddPositionHandlerTests
{
[Fact]
    public async Task Handle_ReturnsOk_WithResultMappedFromDomain()
    {
        // Arrange
        var writer     = new Mock<IPositionWriter>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        writer.Setup(pw => pw.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
              .Returns(Task.CompletedTask);

        unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var handler = new AddPositionHandler(writer.Object, unitOfWork.Object);
        var cmd = new AddPositionCommand(
            Name: "Project Engineer",
            Code: "PE",
            RequiresLicense: true
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var value = result.Value!;
        value.Id.Should().NotBeEmpty();
        value.Name.Should().Be("Project Engineer");
        value.Code.Should().Be("PE");
        value.RequiresLicense.Should().BeTrue();

        writer.Verify(w => w.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        writer.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsWriterWithPositionMappedFromCommand()
    {
        // Arrange
        Position? captured = null;

        var writer = new Mock<IPositionWriter>(MockBehavior.Strict);
        var unitOfWork    = new Mock<IUnitOfWork>(MockBehavior.Strict);

        writer.Setup(pw => pw.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()))
              .Callback<Position, CancellationToken>((p, _) => captured = p)
              .Returns(Task.CompletedTask);

        unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
           .ReturnsAsync(1);

        var handler = new AddPositionHandler(writer.Object, unitOfWork.Object);
        var cmd = new AddPositionCommand(
            Name: "Civil Designer",
            Code: "CD",
            RequiresLicense: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        captured.Should().NotBeNull();
        captured!.Id.Should().NotBeEmpty();
        captured.Name.Should().Be("Civil Designer");
        captured.Code.Should().Be("CD");
        captured.RequiresLicense.Should().BeFalse();

        writer.Verify(w => w.AddAsync(It.IsAny<Position>(), It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        writer.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }
}