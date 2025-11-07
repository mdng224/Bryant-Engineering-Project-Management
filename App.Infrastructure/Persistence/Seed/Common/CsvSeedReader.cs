using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace App.Infrastructure.Persistence.Seed.Common;

internal static class CsvSeedReader
{
    public static List<T> Read<T>(TextReader reader, ClassMap<T> map)
    {
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            DetectDelimiter = true,
            TrimOptions = TrimOptions.Trim,
            BadDataFound = null,
            MissingFieldFound = null,
            IgnoreBlankLines = true,
        };

        using var csv = new CsvReader(reader, config);
        csv.Context.RegisterClassMap(map);
        
        return csv.GetRecords<T>().ToList();
    }
}