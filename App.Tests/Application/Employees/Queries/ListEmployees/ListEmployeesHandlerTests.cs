using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Pagination;
using App.Application.Employees.Queries.ListEmployees;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Employees.Queries.ListEmployees;

 public class ListEmployeesHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_With_EmptyEmployees_And_ZeroTotals()
    {
        // Arrange
        var mockReader = new Mock<IEmployeeReader>();

        mockReader
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var handler = new ListEmployeesHandler(mockReader.Object);
        var pagedQuery = new PagedQuery(page: 1, pageSize: 10);
        var query = new ListEmployeesQuery(pagedQuery, NameFilter: null, IsDeleted: null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var payload = result.Value!;
        payload.Items.Should().BeEmpty();
        payload.TotalCount.Should().Be(0);
        payload.Page.Should().Be(1);
        payload.PageSize.Should().Be(10);
        payload.TotalPages.Should().Be(0);
    }

    [Fact]
    public async Task Handle_Should_Compute_TotalPages_Correctly()
    {
        // Arrange
        var mockReader = new Mock<IEmployeeReader>();

        // total = 25 with empty current page list; pageSize=10 -> totalPages=3
        mockReader
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 25));

        var handler = new ListEmployeesHandler(mockReader.Object);
        var pagedQuery = new PagedQuery(page: 2, pageSize: 10);
        var query = new ListEmployeesQuery(pagedQuery, NameFilter: null, IsDeleted: null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var payload = result.Value!;
        payload.TotalCount.Should().Be(25);
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(10);
        payload.TotalPages.Should().Be(3); // 25 / 10 => 3 pages

        // Verify paging math and null isDeleted passthrough
        mockReader.Verify(r => r.GetPagedAsync(10,
                10,
                null,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Bubble_Repository_Exception()
    {
        // Arrange
        var mockReader = new Mock<IEmployeeReader>();

        mockReader
            .Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new ListEmployeesHandler(mockReader.Object);
        var pagedQuery = new PagedQuery(page: 1, pageSize: 10);
        var query = new ListEmployeesQuery(pagedQuery, NameFilter: "doe", IsDeleted: false);

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act
            .Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*boom*");
    }
}
