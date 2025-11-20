namespace App.Api.Features.Users.ListUsers;

public sealed record ListUsersRequest(
    int Page,
    int PageSize,
    string? EmailFilter,
    bool? IsDeleted
);