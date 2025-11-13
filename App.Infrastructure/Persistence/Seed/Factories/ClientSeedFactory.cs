using CsvHelper.Configuration;
using System.Globalization;
using CsvHelper;

namespace App.Infrastructure.Persistence.Seed.Factories;

public class ClientSeedFactory
{
    public sealed record ClientSeed(
        string? ClientName,
        string? ClientCategory,
        string? ClientType,
        string? NamePrefix,
        string? FirstName,
        string? LastName,
        string? NameSuffix,
        string? Email,
        string? Phone,
        string? Line1,
        string? Line2,
        string? City,
        string? State,
        string? PostalCode,
        string? Note
    );
    
    private sealed class ClientCsvRow
    {
        public string? ClientName { get; set; }          // client_name
        public string? ClientCategory { get; set; }      // CLIENT CATEGORY
        public string? ClientType { get; set; }          // CLIENT TYPE
        public string? NamePrefix { get; set; }          // name_prefix
        public string? FirstName { get; set; }           // first_name
        public string? LastName { get; set; }            // last_name
        public string? NameSuffix { get; set; }          // name_suffix
        public string? Email { get; set; }               // email
        public string? Phone { get; set; }               // phone
        public string? Line1 { get; set; }               // line1
        public string? Line2 { get; set; }               // line2
        public string? City { get; set; }                // city
        public string? State { get; set; }               // state
        public string? PostalCode { get; set; }          // postal_code
        public string? Note { get; set; }                // note
    }

    private sealed class ClientCsvMap : ClassMap<ClientCsvRow>
    {
        public ClientCsvMap()
        {
            Map(ccr => ccr.ClientName)   .Name("client_name", "Client Name");
            Map(ccr => ccr.ClientCategory).Name("CLIENT CATEGORY", "Client Category");
            Map(ccr => ccr.ClientType)   .Name("CLIENT TYPE", "Client Type");
            Map(ccr => ccr.NamePrefix)   .Name("name_prefix", "Name Prefix");
            Map(ccr => ccr.FirstName)    .Name("first_name", "First Name");
            Map(ccr => ccr.LastName)     .Name("last_name", "Last Name");
            Map(ccr => ccr.NameSuffix)   .Name("name_suffix", "Name Suffix");
            Map(ccr => ccr.Email)        .Name("email", "Email");
            Map(ccr => ccr.Phone)        .Name("phone", "Phone");
            Map(ccr => ccr.Line1)        .Name("line1", "Line1", "Address Line 1");
            Map(ccr => ccr.Line2)        .Name("line2", "Line2", "Address Line 2");
            Map(ccr => ccr.City)         .Name("city", "City");
            Map(ccr => ccr.State)        .Name("state", "State");
            Map(ccr => ccr.PostalCode)   .Name("postal_code", "Postal Code", "Zip", "ZIP");
            Map(ccr => ccr.Note)         .Name("note", "Note");
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
            PrepareHeaderForMatch = args => args.Header.Trim().ToLowerInvariant() ?? string.Empty
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap<ClientCsvMap>();

        foreach (var row in csv.GetRecords<ClientCsvRow>())
        {
            yield return new ClientSeed(
                ClientName:     Collapse(row.ClientName),
                ClientCategory: Collapse(row.ClientCategory),
                ClientType:     Collapse(row.ClientType),
                NamePrefix:     NullIfWhiteSpace(row.NamePrefix),
                FirstName:      Collapse(row.FirstName),
                LastName:       Collapse(row.LastName),
                NameSuffix:     NullIfWhiteSpace(row.NameSuffix),
                Email:          NormalizeEmail(row.Email),
                Phone:          NormalizePhone(row.Phone),
                Line1:          Collapse(row.Line1),
                Line2:          NullIfWhiteSpace(row.Line2),
                City:           Collapse(row.City),
                State:          NormalizeState(row.State),
                PostalCode:     NormalizePostal(row.PostalCode),
                Note:           NullIfWhiteSpace(row.Note)
            );
        }
    }
    
    // --- helpers --------------------------------------------------
    private static string? NullIfWhiteSpace(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;
    private static string? Collapse(string? s) =>
        string.IsNullOrWhiteSpace(s)
            ? null
            : System.Text.RegularExpressions.Regex.Replace(s.Trim(), @"\s+", " ");
    private static string? NormalizeEmail(string? s) => NullIfWhiteSpace(s)?.Trim().ToLowerInvariant();
    private static string? NormalizePhone(string? s)
    {
        var digits = new string((NullIfWhiteSpace(s) ?? "").Where(char.IsDigit).ToArray());
        return digits.Length is 10 or 11 ? digits : NullIfWhiteSpace(s); // keep as-is if unusual
    }
    private static string? NormalizeState(string? s) => NullIfWhiteSpace(s)?.ToUpperInvariant();
    private static string? NormalizePostal(string? s) => NullIfWhiteSpace(s)?.ToUpperInvariant();
}