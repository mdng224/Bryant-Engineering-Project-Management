using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace App.Infrastructure.Persistence.Seed.Factories;

internal static class ProjectSeedFactory
{
    public sealed record ProjectSeed(
        string ProjectCode,
        string ProjectName,
        string ClientNameFinal,
        string Scope,
        string PM,
        string Status,
        string Location,
        string Type
    );

    private sealed class ProjectCsvRow
    {
        public string? ProjectCode        { get; set; } // "PROJECT Code"
        public string? ProjectName        { get; set; } // "PROJECT NAME"
        public string? ClientNameFinal    { get; set; } // "Client Name (FINAL)"
        public string? Scope              { get; set; } // "Scope"
        public string? PM                 { get; set; } // "PM"
        public string? Status             { get; set; } // "Status"
        public string? Location           { get; set; } // "Location"
        public string? Type               { get; set; } // "Project Type"
    }

    private sealed class ProjectCsvMap : ClassMap<ProjectCsvRow>
    {
        public ProjectCsvMap()
        {
            Map(pcr => pcr.ProjectCode)     .Name("PROJECT Code","Project Code","Code");
            Map(pcr => pcr.ProjectName)     .Name("PROJECT NAME","Project Name");
            Map(pcr => pcr.ClientNameFinal) .Name("Client Name (FINAL)","Client Name");
            Map(pcr => pcr.Scope)           .Name("Scope");
            Map(pcr => pcr.PM)              .Name("PM","Manager");
            Map(pcr => pcr.Status)          .Name("Status");
            Map(pcr => pcr.Location)        .Name("Location");
            Map(pcr => pcr.Type)            .Name("Project Type","Type");
        }
    }

    public static IEnumerable<ProjectSeed> Enumerate(TextReader reader)
    {
        var cfg = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            MissingFieldFound = null,
            BadDataFound = null,
            DetectColumnCountChanges = true,
            PrepareHeaderForMatch = h => h.Header.Trim().ToLowerInvariant() ?? string.Empty
        };

        using var csv = new CsvReader(reader, cfg);
        csv.Context.RegisterClassMap<ProjectCsvMap>();

        foreach (var row in csv.GetRecords<ProjectCsvRow>())
        {
            var code     = Collapse(row.ProjectCode)     ?? throw new InvalidOperationException("Project code required.");
            var name          = Collapse(row.ProjectName)     ?? "Unknown";
            var client   = Collapse(row.ClientNameFinal) ?? "Unknown";
            var scope    = Collapse(row.Scope)           ?? "Unknown";
            var pm       = Collapse(row.PM)              ?? "Unknown";
            var status   = Collapse(row.Status)          ?? "Unknown";
            var location = Collapse(row.Location)        ?? "Unknown";
            var type     = Collapse(row.Type)            ?? "Unknown";

            yield return new ProjectSeed(code, name, client, scope, pm, status, location, type);
        }
    }

    private static string? Collapse(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
}