using App.Api.Contracts.Users;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Users.Commands.UpdateUser;
using App.Application.Users.Queries;
using App.Domain.Common;

namespace App.Api.Features.Users.Mappers;

internal static class UserMappers
{
    public static GetUsersResponse ToResponse(this GetUsersResult result) =>
        new(
            [
                .. result.Users.Select(u => u.ToResponse())
            ],
            result.TotalCount,
            result.Page,
            result.PageSize,
            result.TotalPages);

    public static GetUsersQuery ToQuery(this GetUsersRequest request)
    {
        var page = request.Page is >= 1
            ? request.Page
            : PagingDefaults.DefaultPage;
        var size = request.PageSize is >= 1 and <= PagingDefaults.MaxPageSize
            ? request.PageSize
            : PagingDefaults.DefaultPageSize;

        var normalizedEmail = request.Email?.ToNormalizedEmail();

        return new GetUsersQuery(page, size, normalizedEmail);
    }

    public static UpdateUserCommand ToCommand(this UpdateUserRequest request, Guid userId)
        => new(userId, request.RoleName, request.Status);
    
    private static UserResponse ToResponse(this UserDto dto) =>
        new(
            Id: dto.Id,
            Email: dto.Email,
            RoleName: dto.RoleName,
            Status: dto.Status,
            CreatedAtUtc: dto.CreatedAtUtc,
            UpdatedAtUtc: dto.UpdatedAtUtc,
            DeletedAtUtc: dto.DeletedAtUtc
        );
}
