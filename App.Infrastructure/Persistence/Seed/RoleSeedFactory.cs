using App.Domain.Security;
using App.Domain.Users;

namespace App.Infrastructure.Persistence.Seed;

internal static class RoleSeedFactory
{
    public static IEnumerable<Role> All =>
    [
        new Role(
            id:   RoleIds.Administrator,
            name: RoleNames.Administrator
        ),
        new Role(
            id:   RoleIds.Manager,
            name: RoleNames.Manager
        ),
        new Role(
            id:   RoleIds.User,
            name: RoleNames.User
        )
    ];
}