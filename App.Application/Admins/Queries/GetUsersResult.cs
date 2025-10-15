using App.Application.Common.Dtos;

namespace App.Application.Admins.Queries;

public sealed record GetUsersResult(
    IReadOnlyList<UserDto> Users,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);
