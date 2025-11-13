using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Positions.Queries.GetPositions;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Positions.Queries.GetPositions;

public class GetPositionsHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_With_Paged_Result_When_Positions_Exist()
    {
        // Arrange
        // Query asks for page 2 with pageSize 1 -> skip should be 1, take should be 1
        var pagedQuery = new PagedQuery(page: 2, pageSize: 1);
        var query = new GetPositionsQuery(pagedQuery, NameFilter: null, IsDeleted: null);

        var pageItems = new List<PositionListItemDto>
        {
            new(
                Guid.NewGuid(),
                "Manager",
                "MGR",
                false,
                null
            )
        };

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.Is<int>(skip => skip == 1),
                It.Is<int>(take => take == 1),
                It.Is<string?>(filter => filter == null),
                It.Is<bool?>(isDeleted => isDeleted == null),
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
                It.Is<string?>(filter => filter == null),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Ok_With_Empty_Payload_When_No_Positions()
    {
        // Arrange
        var pagedQuery = new PagedQuery(page: 1, pageSize: 10);
        var query = new GetPositionsQuery(pagedQuery, NameFilter: null, IsDeleted: false);

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.Is<int>(skip => skip == 0),
                It.Is<int>(take => take == 10),
                It.Is<string?>(filter => filter == null),
                It.Is<bool?>(isDeleted => isDeleted == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PositionListItemDto>(), totalCount: 0));

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
        value.TotalPages.Should().Be(0); // matches PagedResult logic for total == 0

        mockReader.Verify(
            r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 10),
                It.Is<string?>(filter => filter == null),
                It.Is<bool?>(d => d == false),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Pass_Normalized_NameFilter_To_Reader()
    {
        // Arrange
        var pagedQuery = new PagedQuery(page: 1, pageSize: 5);
        var query = new GetPositionsQuery(pagedQuery, NameFilter: "  ProjEcT EnG  ", IsDeleted: true);

        var mockReader = new Mock<IPositionReader>();
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.Is<int>(skip => skip == 0),
                It.Is<int>(take => take == 5),
                It.Is<string?>(filter =>
                    filter != null &&
                    filter.Contains("project", StringComparison.OrdinalIgnoreCase)),
                It.Is<bool?>(isDeleted => isDeleted == true),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<PositionListItemDto>(), totalCount: 0));

        var handler = new GetPositionsHandler(mockReader.Object);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        mockReader.VerifyAll();
    }
}
