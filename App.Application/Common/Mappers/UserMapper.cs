using App.Application.Common.Dtos;
using App.Domain.Security;
using App.Domain.Users;

namespace App.Application.Common.Mappers;

public static class UserMapper
{
    public static IEnumerable<UserDto> ToDto(this IEnumerable<User> users) => users.Select(ToDto);
    private static UserDto ToDto(this User user) =>
        new(
            Id: user.Id,
            Email: user.Email,
            RoleName: user.RoleId.ToName(),
            Status: user.Status.ToString(),
            CreatedAtUtc: user.CreatedAtUtc,
            UpdatedAtUtc: user.UpdatedAtUtc,
            DeletedAtUtc: user.DeletedAtUtc
        );

}