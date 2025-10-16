// App.Api/Mappers/Admins/AdminMappers.cs
using App.Api.Contracts.Admins;
using App.Application.Admins.Commands.UpdateUser;
using App.Application.Admins.Queries;
using App.Application.Common;
using App.Application.Common.Dtos;

namespace App.Api.Mappers.Admins;

internal static class AdminMappers
{
    public static GetUsersResponse ToResponse(this GetUsersResult result)
        => new(
            [.. result.Users.Select(u => u.ToResponse())],
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.TotalPages);

    public static UserResponse ToResponse(this UserDto dto)
        => new(dto.Id, dto.Email, dto.RoleName, dto.IsActive, dto.CreatedAtUtc, dto.UpdatedAtUtc, dto.DeletedAtUtc);

    public static GetUsersQuery ToQuery(this GetUsersRequest request)
    {
        var page = request.Page is >= 1
            ? request.Page
            : PagingDefaults.DefaultPage;
        var size = request.PageSize is >= 1 and <= PagingDefaults.MaxPageSize
            ? request.PageSize
            : PagingDefaults.DefaultPageSize;

        return new GetUsersQuery(page, size);
    }

    public static UpdateUserCommand ToCommand(this UpdateUserRequest request, Guid userId)
        => new(userId, request.RoleName, request.IsActive);
}
