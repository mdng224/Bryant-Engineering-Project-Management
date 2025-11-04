using App.Domain.Security;
using App.Domain.Users;

namespace App.Infrastructure.Persistence.Seed;

internal static class RoleSeedFactory
{
    public static IEnumerable<Role> All =>
    [
        new(id: RoleIds.Administrator, name: RoleNames.Administrator),
        new(id: RoleIds.Manager,       name: RoleNames.Manager),
        new(id: RoleIds.User,          name: RoleNames.User)
    ];
}