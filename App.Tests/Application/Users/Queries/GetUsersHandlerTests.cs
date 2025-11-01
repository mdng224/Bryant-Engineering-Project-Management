using App.Application.Abstractions;
using App.Application.Abstractions.Persistence;
using App.Application.Common.Pagination;
using App.Application.Users.Queries;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Users.Queries;

public sealed class GetUsersHandlerTests
{
    private readonly Mock<IUserReader> _reader = new();
    private readonly GetUsersHandler _handler;

    public GetUsersHandlerTests()
    {
        _handler = new GetUsersHandler(_reader.Object);
    }

    private static List<User> MakeUsers(int count, Guid? roleId = null)
    {
        var list = new List<User>(capacity: count);
        for (var i = 0; i < count; i++)
        {
            var u = new User($"user{i}@example.com", "hash", roleId ?? RoleIds.User);
            list.Add(u);
        }
        return list;
    }

    [Fact]
    public async Task Returns_Paged_Users_With_Normalized_Paging()
    {
        // Arrange: page < 1 -> normalized to 1; pageSize < 1 -> default to 25
        var pagedQuery = new PagedQuery(page: 0, pageSize: 0);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null);

        // Expect skip = (1 - 1) * 25 = 0; take = 25; email = null
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MakeUsers(10), 42));

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.TotalCount.Should().Be(42);
        payload.Page.Should().Be(1);
        payload.PageSize.Should().Be(25);
        payload.Items.Count.Should().Be(10);
        payload.TotalPages.Should().Be((int)Math.Ceiling(42 / 25.0)); // 2
    }

    [Fact]
    public async Task Caps_PageSize_At_100_And_Computes_Skip()
    {
        // Arrange: page=3, requested pageSize=500 -> capped at 100
        var pagedQuery = new PagedQuery(page: 3, pageSize: 500);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null);

        // Expect skip = (3 - 1) * 100 = 200; take = 100; email = null
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 200),
                It.Is<int>(t => t == 100),
                It.Is<string?>(e => e == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MakeUsers(100), 1_234));

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(3);
        payload.PageSize.Should().Be(100);
        payload.Items.Count.Should().Be(100);
        payload.TotalCount.Should().Be(1234);
        payload.TotalPages.Should().Be((int)Math.Ceiling(1234 / 100.0)); // 13
    }

    [Fact]
    public async Task Uses_Provided_PageSize_When_Within_Bounds()
    {
        // Arrange: page=2, pageSize=50 (valid)
        var pagedQuery = new PagedQuery(page: 2, pageSize: 50);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null);

        // Expect skip = (2 - 1) * 50 = 50; take = 50; email = null
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 50),
                It.Is<int>(t => t == 50),
                It.Is<string?>(e => e == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MakeUsers(50), 120));

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(2);
        payload.PageSize.Should().Be(50);
        payload.Items.Count.Should().Be(50);
        payload.TotalPages.Should().Be((int)Math.Ceiling(120 / 50.0)); // 3
    }

    [Fact]
    public async Task TotalPages_Is_Zero_When_Total_Is_Zero()
    {
        var pagedQuery = new PagedQuery(page: 5, pageSize: 25);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null);

        _reader.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.Is<string?>(e => e == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<User>(), 0));

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.TotalCount.Should().Be(0);
        payload.Items.Should().BeEmpty();
        payload.TotalPages.Should().Be(0);
        payload.Page.Should().Be(5);
        payload.PageSize.Should().Be(25);
    }

    [Fact]
    public async Task Normalizes_Negative_Page_To_1_And_Defaults_PageSize_To_25()
    {
        var pagedQuery = new PagedQuery(page: -10, pageSize: -3);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null);

        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),   // (1-1)*25
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MakeUsers(3), 3));

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(1);
        payload.PageSize.Should().Be(25);
        payload.TotalPages.Should().Be(1);
        payload.Items.Count.Should().Be(3);
    }

    // ---------- New tests for email search ----------

    [Fact]
    public async Task Applies_Email_Filter_When_Provided()
    {
        var pagedQuery = new PagedQuery(page: 1, pageSize: 25);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: "dan");

        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == "dan"),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((MakeUsers(2), 2));

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(1);
        payload.PageSize.Should().Be(25);
        payload.Items.Count.Should().Be(2);
        payload.TotalCount.Should().Be(2);
        payload.TotalPages.Should().Be(1);
    }

    [Fact]
    public async Task Trims_Email_Filter_Before_Passing_To_Reader()
    {
        var pagedQuery = new PagedQuery(page: 1, pageSize: 25);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: "  dan  ");

        string? passedEmail = null;

        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, int, string?, CancellationToken>((s, t, e, ct) => passedEmail = e)
            .ReturnsAsync((MakeUsers(1), 1));

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        passedEmail.Should().Be("dan"); // verify the trimming actually happened

        var payload = res.Value!;
        payload.Items.Should().NotBeNull();
        payload.Items.Count.Should().Be(1);
        payload.TotalCount.Should().Be(1);
    }
}
