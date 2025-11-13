using System.Globalization;
using App.Infrastructure.Persistence.Seed.Common;
using CsvHelper;
using CsvHelper.Configuration;

namespace App.Infrastructure.Persistence.Seed.Factories;

internal static class ClientProjectSeedFactory
{
    // ---------- Public seed records (what DbSeeder will use) ------------------
    public sealed record CombinedSeed(
        string ProjectCode,
        string ProjectName,
        string ClientName,
        string ProjectContact,
        string Scope,
        string PM,
        string Status,
        string Location,
        string Type
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
        public string Type            { get; set; } = ""; // "Project Type"
    }

    private sealed class CombinedCsvMap : ClassMap<CombinedCsvRow>
    {
        public CombinedCsvMap()
        {
            Map(ccr => ccr.ProjectCode)     .Name("PROJECT Code");
            Map(ccr => ccr.ProjectName)     .Name("PROJECT NAME");
            Map(ccr => ccr.ClientNameFinal) .Name("Client Name (FINAL)");
            Map(ccr => ccr.ProjectContact)  .Name("PROJECT CONTACT");
            Map(ccr => ccr.Scope)           .Name("Scope");
            Map(ccr => ccr.PM)              .Name("PM");
            Map(ccr => ccr.Status)          .Name("Status");
            Map(ccr => ccr.Location)        .Name("Location");
            Map(ccr => ccr.Type)            .Name("Project Type");
        }
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
                ProjectName   : SeedUtils.CollapseSpaces(ccr.ProjectName) ?? "unknown",
                ClientName    : SeedUtils.CollapseSpaces(ccr.ClientNameFinal) ?? "unknown",
                ProjectContact: SeedUtils.NullIfWhiteSpace(ccr.ProjectContact) ?? "unknown",
                Scope         : SeedUtils.NullIfWhiteSpace(ccr.Scope) ?? "unknown",
                PM            : SeedUtils.NullIfWhiteSpace(ccr.PM) ?? "unknown",
                Status        : SeedUtils.NullIfWhiteSpace(ccr.Status) ?? "unknown",
                Location      : SeedUtils.CollapseSpaces(ccr.Location) ?? "unknown",
                Type          : SeedUtils.CollapseSpaces(ccr.Type) ?? "unknown"
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