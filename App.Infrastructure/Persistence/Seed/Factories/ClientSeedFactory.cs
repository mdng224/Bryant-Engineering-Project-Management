using CsvHelper.Configuration;
using System.Globalization;
using App.Domain.Common;
using CsvHelper;

namespace App.Infrastructure.Persistence.Seed.Factories;

public class ClientSeedFactory
{
    public sealed record ClientSeed(
        string? ClientName,
        string? ClientCategory,
        string? ClientType
    );
    
    private sealed class ClientCsvRow
    {
        public string? ClientName { get; set; }          // client_name
        public string? ClientCategory { get; set; }      // CLIENT CATEGORY
        public string? ClientType { get; set; }          // CLIENT TYPE
    }

    private sealed class ClientCsvMap : ClassMap<ClientCsvRow>
    {
        public ClientCsvMap()
        {
            Map(ccr => ccr.ClientName)   .Name("client_name", "Client Name");
            Map(ccr => ccr.ClientCategory).Name("CLIENT CATEGORY", "Client Category");
            Map(ccr => ccr.ClientType)   .Name("CLIENT TYPE", "Client Type");
        }
    }
    
    public static IEnumerable<ClientSeed> Enumerate(TextReader reader)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true,
            BadDataFound = null,
            MissingFieldFound = null,
            DetectColumnCountChanges = true,
            

            // Make header matching forgiving (so "CLIENT CATEGORY" == "client category")
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant()
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<ClientCsvMap>();

        foreach (var row in csv.GetRecords<ClientCsvRow>())
        {
            var clientName  = string.IsNullOrWhiteSpace(row.ClientName)
                ? "Unknown"
                : row.ClientName.ToProperCase();
            
            yield return new ClientSeed(
                ClientName:     clientName,
                ClientCategory: Collapse(row.ClientCategory),
                ClientType:     Collapse(row.ClientType)
            );
        }
    }
    
    // --- helpers --------------------------------------------------
    private static string? Collapse(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
}