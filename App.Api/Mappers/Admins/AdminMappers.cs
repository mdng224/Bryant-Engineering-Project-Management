// App.Api/Mappers/Admins/AdminMappers.cs
using App.Api.Contracts.Admins;
using App.Application.Admins.Commands.PatchUser;
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

        // normalize: trim, then null if empty
        var email = string.IsNullOrWhiteSpace(request.Email)
            ? null
            : request.Email.Trim();

        return new GetUsersQuery(page, size, email);
    }

    public static PatchUserCommand ToCommand(this PatchUserRequest request, Guid userId)
        => new(userId, request.RoleName, request.IsActive);
}
