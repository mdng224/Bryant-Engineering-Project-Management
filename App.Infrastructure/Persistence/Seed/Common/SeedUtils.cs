namespace App.Infrastructure.Persistence.Seed.Common;

internal static class SeedUtils
{
    public static string? CollapseSpaces(string? s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return null;
        var span = s.AsSpan();
        var sb = new System.Text.StringBuilder(span.Length);
        var lastSpace = false;
        
        foreach (var ch in span)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (lastSpace) continue;
                sb.Append(' '); lastSpace = true;
            }
            else
            {
                sb.Append(ch);
                lastSpace = false;
            }
        }
        return sb.ToString().Trim().ToLowerInvariant();
    }
    
    public static string? NullIfWhiteSpace(string? s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}