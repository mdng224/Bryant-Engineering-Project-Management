using App.Application.Abstractions;
using App.Application.Positions.Queries;
using App.Domain.Employees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Queries;

public class GetAllPositionsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_When_Positions_Exist()
    {
        // Arrange
        var positions = new List<Position>
        {
            new("Engineer", "ENG", false),
            new("Manager", "MGR", true)
        };

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(positions);

        var handler = new GetAllPositionsHandler(mockReader.Object);

        // Act
        var result = await handler.Handle(new GetAllPositionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var value = result.Value!;
        value.Positions.Should().HaveCount(2);
        value.Positions.Should().ContainSingle(p => p.Name == "Engineer");
        value.Positions.Should().ContainSingle(p => p.Name == "Manager");
        value.Positions.Should().AllSatisfy(dto =>
        {
            dto.Id.Should().NotBeEmpty();
            dto.Name.Should().NotBeNullOrWhiteSpace();
        });

        mockReader.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Fail_When_No_Positions_Found()
    {
        // Arrange
        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Position>());

        var handler = new GetAllPositionsHandler(mockReader.Object);

        // Act
        var result = await handler.Handle(new GetAllPositionsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNull();
        result.Error!.Value.Code.Should().Be("no_positions");
        result.Error!.Value.Message.Should().Be("No positions found.");

        mockReader.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}