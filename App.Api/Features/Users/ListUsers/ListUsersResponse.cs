namespace App.Api.Features.Users.ListUsers;

public sealed record ListUsersResponse(
    IReadOnlyList<UserRowResponse> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
