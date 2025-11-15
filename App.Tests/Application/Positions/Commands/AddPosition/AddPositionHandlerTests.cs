using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Positions.Commands.AddPosition;
using App.Domain.Employees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Commands.AddPosition;

public class AddPositionHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOk_WithNewPositionId_WhenNewPositionCreated()
    {
        // Arrange
        var reader     = new Mock<IPositionReader>(MockBehavior.Strict);
        var writer     = new Mock<IPositionRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        // No existing matches -> create path
        reader.Setup(pr => pr.GetByNameIncludingDeletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync([]);

        writer.Setup(pw => pw.Add(It.IsAny<Position>()));

        unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

        var handler = new AddPositionHandler(reader.Object, writer.Object, unitOfWork.Object);

        var cmd = new AddPositionCommand(
            Name: "Project Engineer",
            Code: "PE",
            RequiresLicense: true
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        reader.Verify(pr => pr.GetByNameIncludingDeletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(pw => pw.Add(It.IsAny<Position>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        reader.VerifyNoOtherCalls();
        writer.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Handle_CallsWriterWithPositionMappedFromCommand_AndReturnsItsId()
    {
        // Arrange
        Position? captured = null;

        var reader     = new Mock<IPositionReader>(MockBehavior.Strict);
        var writer     = new Mock<IPositionRepository>(MockBehavior.Strict);
        var unitOfWork = new Mock<IUnitOfWork>(MockBehavior.Strict);

        reader.Setup(pr => pr.GetByNameIncludingDeletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
              .ReturnsAsync([]);

        writer.Setup(pw => pw.Add(It.IsAny<Position>()))
              .Callback<Position>(p => captured = p);

        unitOfWork.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

        var handler = new AddPositionHandler(reader.Object, writer.Object, unitOfWork.Object);

        var cmd = new AddPositionCommand(
            Name: "Civil Designer",
            Code: "CD",
            RequiresLicense: false
        );

        // Act
        var result = await handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        captured.Should().NotBeNull();
        captured!.Id.Should().NotBe(Guid.Empty);
        captured.Name.Should().Be("Civil Designer");
        captured.Code.Should().Be("CD");
        captured.RequiresLicense.Should().BeFalse();

        // Returned id should match the created position's id
        result.Value.Should().Be(captured.Id);

        reader.Verify(pr => pr.GetByNameIncludingDeletedAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        writer.Verify(pw => pw.Add(It.IsAny<Position>()), Times.Once);
        unitOfWork.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        reader.VerifyNoOtherCalls();
        writer.VerifyNoOtherCalls();
        unitOfWork.VerifyNoOtherCalls();
    }
}
