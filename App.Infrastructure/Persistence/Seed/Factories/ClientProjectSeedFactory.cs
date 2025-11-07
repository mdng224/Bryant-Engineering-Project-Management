using System.Globalization;
using App.Infrastructure.Persistence.Seed.Common;
using CsvHelper;
using CsvHelper.Configuration;

namespace App.Infrastructure.Persistence.Seed.Factories;

public static class ClientProjectSeedFactory
{
    // ---------- Public seed records (what DbSeeder will use) ------------------

    public sealed record CombinedSeed(
        string  ProjectCode,
        string? ProjectName,
        string? ClientName,
        string? ProjectContact,
        string? Scope,
        string? PM,
        string? Status,
        string? Location
    );

    public sealed record ClientSeed(string ClientName, string? ContactRaw, string? FirstProjectCode);
    public sealed record ProjectSeed(
        string  ProjectCode,
        string? ProjectName,
        string? Scope,
        string? PM,
        string? Status,
        string? Location
    );

    // ---------- CSV row + map (private) --------------------------------------

    private sealed class CombinedCsvRow
    {
        public string ProjectCode     { get; set; } = "";  // "PROJECT Code"
        public string ProjectName     { get; set; } = "";  // "PROJECT NAME"
        public string ClientNameFinal { get; set; } = "";  // "Client Name (FINAL)"
        public string ProjectContact  { get; set; } = "";  // "PROJECT CONTACT"
        public string Scope           { get; set; } = "";  // "Scope"
        public string PM              { get; set; } = "";  // "PM"
        public string Status          { get; set; } = "";  // "Status"
        public string Location        { get; set; } = "";  // "Location"
    }

    private sealed class CombinedCsvMap : ClassMap<CombinedCsvRow>
    {
        public CombinedCsvMap()
        {
            Map(x => x.ProjectCode)     .Name("PROJECT Code");
            Map(x => x.ProjectName)     .Name("PROJECT NAME");
            Map(x => x.ClientNameFinal) .Name("Client Name (FINAL)");
            Map(x => x.ProjectContact)  .Name("PROJECT CONTACT");
            Map(x => x.Scope)           .Name("Scope");
            Map(x => x.PM)              .Name("PM");
            Map(x => x.Status)          .Name("Status");
            Map(x => x.Location)        .Name("Location");
        }
    }

    // ---------- Core loaders --------------------------------------------------

    /// Unique clients by name (case-insensitive). Keeps the first contact & first project code seen.
    public static List<ClientSeed> UniqueClientsByName(List<CombinedSeed> allRows, out List<string> warnings)
    {
        warnings = [];
        var normalized = allRows
            .Where(r => !string.IsNullOrWhiteSpace(r.ClientName))
            .Select(r => new
            {
                ClientName = r.ClientName!,
                ContactRaw = r.ProjectContact,
                r.ProjectCode
            })
            .ToList();

        // Keep first row per client (case-insensitive)
        var dict = new Dictionary<string, ClientSeed>(StringComparer.OrdinalIgnoreCase);
        foreach (var r in normalized.Where(r => !dict.ContainsKey(r.ClientName)))
        {
            dict[r.ClientName] = new ClientSeed(r.ClientName, r.ContactRaw, r.ProjectCode);
        }

        var duplicates = normalized.Count - dict.Count;
        if (duplicates > 0)
            warnings.Add($"Dropped {duplicates} duplicate client row(s) by Client Name.");

        return dict.Values.ToList();
    }

    /// Unique projects by project code (case-insensitive). Keeps first.
    public static List<ProjectSeed> UniqueProjectsByCode(List<CombinedSeed> allRows, out List<string> warnings)
    {
        warnings = [];

        var normalized = allRows
            .Where(cs => !string.IsNullOrWhiteSpace(cs.ProjectCode))
            .Select(cs => new ProjectSeed(
                ProjectCode: cs.ProjectCode!,
                ProjectName: cs.ProjectName,
                Scope      : cs.Scope,
                PM         : cs.PM,
                Status     : cs.Status,
                Location   : cs.Location
            ))
            .ToList();

        var unique = SeedUtils.DedupeBy(
            normalized,
            keySelector: p => p.ProjectCode,
            comparer: StringComparer.OrdinalIgnoreCase
        ).ToList();

        var duplicates = normalized.Count - unique.Count;
        if (duplicates > 0)
            warnings.Add($"Dropped {duplicates} duplicate project row(s) by PROJECT Code.");

        return unique;
    }

    // ---------- Convenience (single-pass enumerator) --------------------------

    /// Efficient iterator: yields normalized rows in file order. Great for one-loop seeding.
    public static IEnumerable<CombinedSeed> Enumerate(TextReader reader)
    {
        // Using CsvHelper directly to stream
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null,
            DetectColumnCountChanges = true
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<CombinedCsvMap>();

        foreach (var ccr in csv.GetRecords<CombinedCsvRow>())
        {
            yield return new CombinedSeed(
                ProjectCode   : NormalizeCode(ccr.ProjectCode),
                ProjectName   : SeedUtils.CollapseSpaces(ccr.ProjectName),
                ClientName    : SeedUtils.CollapseSpaces(ccr.ClientNameFinal),
                ProjectContact: SeedUtils.NullIfWhiteSpace(ccr.ProjectContact),
                Scope         : SeedUtils.NullIfWhiteSpace(ccr.Scope),
                PM            : SeedUtils.NullIfWhiteSpace(ccr.PM),
                Status        : SeedUtils.NullIfWhiteSpace(ccr.Status),
                Location      : SeedUtils.CollapseSpaces(ccr.Location)
            );
        }
    }

    // ---------- Helpers -------------------------------------------------------

    public static (string? FirstName, string? LastName) TrySplitLastFirst(string? contactRaw)
    {
        if (string.IsNullOrWhiteSpace(contactRaw))
            return (null, null);

        // Handle multi-contact cells like "DOE, JANE & SMITH, JOHN"
        var firstChunk = contactRaw
            .Split(['&', '/', ';'], count: 2, StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim();

        // Expect "LAST, FIRST" format
        var parts = firstChunk.Split(',', count: 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return (null, null);

        var lastName = parts[0];
        var firstName = parts[1];

        return (firstName, lastName);
    }

    private static string NormalizeCode(string? raw) =>
        (SeedUtils.CollapseSpaces(raw) ?? string.Empty)
        .Replace('—', '-')
        .Replace('–', '-')
        .Trim();
}