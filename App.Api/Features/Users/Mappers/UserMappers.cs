using App.Api.Contracts.Users;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Users.Commands.UpdateUser;
using App.Application.Users.Queries;
using App.Domain.Common;

namespace App.Api.Features.Users.Mappers;

internal static class UserMappers
{
    public static GetUsersResponse ToResponse(this PagedResult<UserDto> pagedResult) =>
        new(
            [
                .. pagedResult.Items.Select(u => u.ToResponse())
            ],
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalPages);

    public static GetUsersQuery ToQuery(this GetUsersRequest request)
    {
        var (page, pageSize) = request.PagedRequest;
        var pagedQuery = new PagedQuery(page, pageSize);
        var normalizedEmail = request.Email?.ToNormalizedEmail();

        return new GetUsersQuery(pagedQuery, normalizedEmail);
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
