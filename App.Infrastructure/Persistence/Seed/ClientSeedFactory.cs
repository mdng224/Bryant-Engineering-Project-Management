using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace App.Infrastructure.Persistence.Seed;

public static class ClientSeedFactory
{
    // Only the columns we need for client seeding
    private sealed class ClientCsvRow
    {
        public string ClientNameFinal { get; set; } = "";
        public string ProjectContact  { get; set; } = "";
    }

    // Map headers → properties (exact header text)
    private sealed class ClientCsvMap : ClassMap<ClientCsvRow>
    {
        public ClientCsvMap()
        {
            Map(ccr => ccr.ClientNameFinal).Name("Client Name (FINAL)");
            Map(ccr => ccr.ProjectContact) .Name("PROJECT CONTACT");
        }
    }

    /// <summary>Normalized shape used to create client domain entities.</summary>
    public sealed record ClientSeed(
        string ClientName,
        string? ContactRaw
    );

    /// <summary>
    /// Returns unique clients, deduped by ClientName (case-insensitive).
    /// Keeps the first occurrence for ContactRaw.
    /// </summary>
    public static List<ClientSeed> LoadUniqueByClientName(out List<string> warnings)
    {
        var all = LoadAll(out warnings);

        var unique = all
            .Where(s => !string.IsNullOrWhiteSpace(s.ClientName))
            .GroupBy(s => s.ClientName, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToList();

        var dupes = all.Count - unique.Count;
        if (dupes > 0)
            warnings.Add($"Deduped {dupes} row(s) by Client Name.");

        return unique;
    }

    /// <summary>
    /// Splits "LAST, FIRST" (keeps first contact if multiple like "DOE, JANE & SMITH, JOHN").
    /// Returns (null, null) when format doesn’t match.
    /// </summary>
    public static (string? FirstName, string? LastName) TrySplitLastFirst(string? contactRaw)
    {
        if (string.IsNullOrWhiteSpace(contactRaw))
            return (null, null);

        // If multiple contacts in one cell, keep first: “DOE, JANE & SMITH, JOHN”
        var firstChunk = contactRaw
            .Split(['&', '/', ';'], count: 2, StringSplitOptions.RemoveEmptyEntries)[0]
            .Trim();

        var parts = firstChunk.Split(',', count: 2, StringSplitOptions.TrimEntries);
        if (parts.Length != 2)
            return (null, null);

        var lastName  = parts[0];
        var firstName = parts[1];
        return (firstName, lastName);
    }

    /// <summary>Loads ALL rows from Clients.csv (may include duplicates by client name).</summary>
    private static List<ClientSeed> LoadAll(out List<string> warnings)
    {
        warnings = [];

        var asm = typeof(ClientSeedFactory).Assembly;
        const string resourceName = "App.Infrastructure.Persistence.Seed.Data.ClientsProjects.csv";

        using var stream = asm.GetManifestResourceStream(resourceName)
                           ?? throw new FileNotFoundException(
                               $"Embedded resource '{resourceName}' not found. " +
                               $"Check Build Action = EmbeddedResource and namespace path.");

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

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
        csv.Context.RegisterClassMap<ClientCsvMap>();

        var rows = csv.GetRecords<ClientCsvRow>().ToList();

        var seeds = rows.Select(r => new ClientSeed(
            ClientName: Normalize(r.ClientNameFinal),
            ContactRaw:  NormalizeOrNull(r.ProjectContact)
        )).ToList();

        var missingNameCount = seeds.Count(s => string.IsNullOrWhiteSpace(s.ClientName));
        if (missingNameCount > 0)
            warnings.Add($"Found {missingNameCount} row(s) with empty Client Name (FINAL). Skipped in unique view.");

        return seeds;
    }

    private static string Normalize(string? s) => (s ?? string.Empty).Trim();

    private static string? NormalizeOrNull(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return null;
        var t = str.Trim();
        return t.Length == 0 ? null : t;
    }
}
