using System.Text.RegularExpressions;

namespace App.Domain.Common.ValueObjects;

public readonly record struct ProjectCode(string Code, int Year, int Number)
{
    public static ProjectCode Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new FormatException("Project code cannot be empty.");

        var trimmed = raw.Trim();

        // Legacy: YY-NNNN
        var legacy = Regex.Match(trimmed, @"^(?<yy>\d{2})-(?<num>\d{4})$");
        if (legacy.Success)
        {
            var yy = int.Parse(legacy.Groups["yy"].Value);
            var num = int.Parse(legacy.Groups["num"].Value);
            var year = NormalizeYear(yy);
            return new ProjectCode($"{yy:00}-{num:0000}", year, num);
        }

        // Modern: YYYY-NNNNN
        var modern = Regex.Match(trimmed, @"^(?<yyyy>\d{4})-(?<num>\d{4,5})$");
        if (modern.Success)
        {
            var yyyy = int.Parse(modern.Groups["yyyy"].Value);
            var num = int.Parse(modern.Groups["num"].Value);
            return new ProjectCode($"{yyyy}-{num:0000}", yyyy, num);
        }

        throw new FormatException($"Invalid project code format: {raw}");
    }

    private static int NormalizeYear(int yy) => yy <= 49 ? 2000 + yy : 1900 + yy;

    public static bool TryParse(string? raw, out ProjectCode code)
    {
        try { code = Parse(raw); return true; }
        catch { code = default; return false; }
    }

    public override string ToString() => Code;
}
