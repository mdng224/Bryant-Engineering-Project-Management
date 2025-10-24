namespace App.Api.Contracts.Users;

public sealed record GetUsersResponse(
    IReadOnlyList<UserResponse> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
