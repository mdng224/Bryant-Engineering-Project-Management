using App.Application.Abstractions.Persistence;
using App.Application.Common.Pagination;
using App.Application.Positions.Queries.GetPositions;
using App.Domain.Employees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Queries.GetPositions;

public class GetPositionsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_With_Paged_Result_When_Positions_Exist()
    {
        // Arrange
        // Query asks for page 2 with pageSize 1 -> skip should be 1, take 1
        var pagedQuery = new PagedQuery(page: 2, pageSize: 1);
        var query = new GetPositionsQuery(pagedQuery);

        var pageItems = new List<Position>
        {
            new("Manager", "MGR", true)
        };

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.Is<int>(skip => skip == 1),
                It.Is<int>(take => take == 1),
                It.IsAny<CancellationToken>()))
            // total = 2 to yield TotalPages = 2 with pageSize 1
            .ReturnsAsync((pageItems, totalCount: 2));

        var handler = new GetPositionsHandler(mockReader.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var value = result.Value!;
        value.Items.Should().HaveCount(1);
        value.Items[0].Name.Should().Be("Manager");
        value.TotalCount.Should().Be(2);
        value.Page.Should().Be(2);
        value.PageSize.Should().Be(1);
        value.TotalPages.Should().Be(2); // ceil(2 / 1)

        value.Items.Should().AllSatisfy(dto =>
        {
            dto.Id.Should().NotBeEmpty();
            dto.Name.Should().NotBeNullOrWhiteSpace();
            dto.Code.Should().NotBeNullOrWhiteSpace();
        });

        mockReader.Verify(
            r => r.GetPagedAsync(
                It.Is<int>(s => s == 1),
                It.Is<int>(t => t == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_With_Empty_Payload_When_No_Positions()
    {
        // Arrange
        var pagedQuery = new PagedQuery(page: 1, pageSize: 10);
        var query = new GetPositionsQuery(pagedQuery);

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.Is<int>(skip => skip == 0),
                It.Is<int>(take => take == 10),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Position>(), totalCount: 0));

        var handler = new GetPositionsHandler(mockReader.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var value = result.Value!;
        value.Items.Should().BeEmpty();
        value.TotalCount.Should().Be(0);
        value.Page.Should().Be(1);
        value.PageSize.Should().Be(10);
        value.TotalPages.Should().Be(0); // matches handler logic for total == 0

        mockReader.Verify(
            r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 10),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
