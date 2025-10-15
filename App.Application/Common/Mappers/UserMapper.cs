using App.Application.Common.Dtos;
using App.Domain.Security;
using App.Domain.Users;

namespace App.Application.Common.Mappers;

public static class UserMapper
{
    public static UserDto ToDto(this User user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            RoleName: user.RoleId.ToName(),
            IsActive: user.IsActive,
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc,
            DeletedAtUtc: user.DeletedAtUtc
        );

    public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users) => users.Select(ToDto);
}