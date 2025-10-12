namespace App.Domain.Security;

public static class RoleIds
{
    public static readonly Guid Administrator = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid Manager = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid User = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

    // exact (case-sensitive) mapping – matches your “verbatim” choice
    private static readonly IReadOnlyDictionary<string, Guid> NameToId =
        new Dictionary<string, Guid>(StringComparer.Ordinal)
        {
            [RoleNames.Administrator] = Administrator,
            [RoleNames.Manager] = Manager,
            [RoleNames.User] = User,
        };

    public static bool TryFromName(string roleName, out Guid roleId)
        => NameToId.TryGetValue(roleName, out roleId);
}