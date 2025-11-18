using System.Globalization;
using App.Domain.Common;
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
            PrepareHeaderForMatch = h => h.Header.Trim().ToLowerInvariant()
        };

        using var csv = new CsvReader(reader, cfg);
        csv.Context.RegisterClassMap<ProjectCsvMap>();

        foreach (var row in csv.GetRecords<ProjectCsvRow>())
        {
            var code = Collapse(row.ProjectCode)
                       ?? throw new InvalidOperationException("Project code required.");

            var name = string.IsNullOrWhiteSpace(row.ProjectName)
                ? "Unknown"
                : row.ProjectName.ToProperCase();

            var client = string.IsNullOrWhiteSpace(row.ClientNameFinal)
                ? "Unknown"
                : Collapse(row.ClientNameFinal)!;

            var scope = string.IsNullOrWhiteSpace(row.Scope)
                ? "Unknown"
                : Collapse(row.Scope)!;

            var pm = string.IsNullOrWhiteSpace(row.PM)
                ? "Unknown"
                : row.PM.ToProperCase();

            var status = string.IsNullOrWhiteSpace(row.Status)
                ? "Unknown"
                : Collapse(row.Status)!;

            var location = string.IsNullOrWhiteSpace(row.Location)
                ? "Unknown"
                : row.Location.ToProperCase();

            var type = string.IsNullOrWhiteSpace(row.Type)
                ? "Unknown"
                : Collapse(row.Type)!;

            yield return new ProjectSeed(code, name, client, scope, pm, status, location, type);
        }
    }

    private static string? Collapse(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
}