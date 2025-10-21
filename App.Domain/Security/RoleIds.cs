using System.Collections.Immutable;

namespace App.Domain.Security;

public static class RoleIds
{
    public static readonly Guid Administrator = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid Manager       = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid User          = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly ImmutableDictionary<string, Guid> NameToId =
    ImmutableDictionary.CreateRange(
        StringComparer.Ordinal,
        [
            new KeyValuePair<string, Guid>(RoleNames.Administrator, Administrator),
            new KeyValuePair<string, Guid>(RoleNames.Manager,       Manager),
            new KeyValuePair<string, Guid>(RoleNames.User,          User),
        ]);

    private static readonly ImmutableDictionary<Guid, string> IdToName =
        NameToId.ToImmutableDictionary(kvp => kvp.Value, kvp => kvp.Key);

    public static bool TryFromName(string roleName, out Guid roleId) =>
        NameToId.TryGetValue(roleName, out roleId);

    public static string ToName(this Guid roleId) =>
        CollectionExtensions.GetValueOrDefault(IdToName, roleId, "Unknown");
}