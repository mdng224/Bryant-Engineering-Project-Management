using App.Application.Abstractions.Persistence;
using App.Application.Employees.Queries;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Employees.Queries;

 public class GetEmployeesHandlerTests
{
    [Fact]
    public async Task Handle_Should_Return_Ok_With_EmptyEmployees_And_ZeroTotals()
    {
        // Arrange
        var mockReader = new Mock<IEmployeeReader>();

        // Return an empty list with 0 total
        mockReader
            .Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 0));

        var handler = new GetEmployeesHandler(mockReader.Object);

        var query = new GetEmployeesQuery(Page: 1, PageSize: 10, Name: null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var payload = result.Value;
        payload.Should().NotBeNull();

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
            .Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(([], 25));

        var handler = new GetEmployeesHandler(mockReader.Object);

        var query = new GetEmployeesQuery(Page: 2, PageSize: 10, Name: null);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var payload = result.Value!;
        payload.TotalCount.Should().Be(25);
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(10);
        payload.TotalPages.Should().Be(3); // 25 / 10 => 3 pages
    }

    [Fact]
    public async Task Handle_Should_Bubble_Repository_Exception()
    {
        // Arrange
        var mockReader = new Mock<IEmployeeReader>();

        mockReader
            .Setup(r => r.GetPagedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("boom"));

        var handler = new GetEmployeesHandler(mockReader.Object);

        var query = new GetEmployeesQuery(Page: 1, PageSize: 10, Name: "doe");

        // Act
        Func<Task> act = async () => await handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*boom*");
    }
}
