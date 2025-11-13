using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Users.Queries;
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

    private static List<UserDto> MakeUserDtos(int count)
    {
        var list = new List<UserDto>(capacity: count);
        for (var i = 0; i < count; i++)
        {
            list.Add(new UserDto(
                Id: Guid.NewGuid(),
                Email: $"user{i}@example.com",
                RoleName: "User",
                Status: UserStatus.Active,
                CreatedAtUtc: DateTime.UtcNow,
                UpdatedAtUtc: DateTime.UtcNow,
                DeletedAtUtc: null
            ));
        }
        return list;
    }

    [Fact]
    public async Task Returns_Paged_Users_With_Normalized_Paging()
    {
        // Arrange: page < 1 -> normalized to 1; pageSize < 1 -> default to 25
        var pagedQuery = new PagedQuery(page: 0, pageSize: 0);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null, IsDeleted: null);

        // Expect skip = 0; take = 25; email = null; isDeleted = null
        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(10), 42);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == null),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

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
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null, IsDeleted: null);

        // Expect skip = 200; take = 100; email = null; isDeleted = null
        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(100), 1_234);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 200),
                It.Is<int>(t => t == 100),
                It.Is<string?>(e => e == null),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

        // Act
        var res = await _handler.Handle(query, CancellationToken.None);

        // Assert
        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(3);
        payload.PageSize.Should().Be(100);
        payload.Items.Count.Should().Be(100);
        payload.TotalCount.Should().Be(1_234);
        payload.TotalPages.Should().Be((int)Math.Ceiling(1234 / 100.0)); // 13
    }

    [Fact]
    public async Task Uses_Provided_PageSize_When_Within_Bounds()
    {
        // Arrange: page=2, pageSize=50 (valid)
        var pagedQuery = new PagedQuery(page: 2, pageSize: 50);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null, IsDeleted: null);

        // Expect skip = 50; take = 50; email = null; isDeleted = null
        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(50), 120);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 50),
                It.Is<int>(t => t == 50),
                It.Is<string?>(e => e == null),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

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
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null, IsDeleted: false);

        var resultTuple = ((IReadOnlyList<UserDto>)new List<UserDto>(), 0);
        _reader.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.Is<string?>(e => e == null),
                It.Is<bool?>(d => d == false),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

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
        var query = new GetUsersQuery(pagedQuery, EmailFilter: null, IsDeleted: null);

        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(3), 3);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),   // (1-1)*25
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == null),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        var payload = res.Value!;
        payload.Page.Should().Be(1);
        payload.PageSize.Should().Be(25);
        payload.TotalPages.Should().Be(1);
        payload.Items.Count.Should().Be(3);
    }

    // ---------- Email search ----------

    [Fact]
    public async Task Applies_Email_Filter_When_Provided()
    {
        var pagedQuery = new PagedQuery(page: 1, pageSize: 25);
        var query = new GetUsersQuery(pagedQuery, EmailFilter: "dan", IsDeleted: null);

        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(2), 2);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.Is<string?>(e => e == "dan"),
                It.Is<bool?>(d => d == null),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(resultTuple);

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
        var query = new GetUsersQuery(pagedQuery, EmailFilter: "  dan  ", IsDeleted: true);

        string? passedEmail = null;
        bool? passedDeleted = null;

        var resultTuple = ((IReadOnlyList<UserDto>)MakeUserDtos(1), 1);
        _reader.Setup(r => r.GetPagedAsync(
                It.Is<int>(s => s == 0),
                It.Is<int>(t => t == 25),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .Callback<int, int, string?, bool?, CancellationToken>((s, t, e, d, ct) =>
            {
                passedEmail = e;
                passedDeleted = d;
            })
            .ReturnsAsync(resultTuple);

        var res = await _handler.Handle(query, CancellationToken.None);

        res.IsSuccess.Should().BeTrue();
        passedEmail.Should().Be("dan");   // ToNormalizedEmail trims/normalizes
        passedDeleted.Should().BeTrue();  // IsDeleted passthrough verified

        var payload = res.Value!;
        payload.Items.Should().NotBeNull();
        payload.Items.Count.Should().Be(1);
        payload.TotalCount.Should().Be(1);
    }
}
