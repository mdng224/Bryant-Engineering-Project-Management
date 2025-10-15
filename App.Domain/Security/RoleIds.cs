using System.Collections.Immutable;

namespace App.Domain.Security;

public static class RoleIds
{
    public static readonly Guid Administrator = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    public static readonly Guid Manager = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
    public static readonly Guid User = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

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
        IdToName.TryGetValue(roleId, out var name) ? name : "Unknown";
}