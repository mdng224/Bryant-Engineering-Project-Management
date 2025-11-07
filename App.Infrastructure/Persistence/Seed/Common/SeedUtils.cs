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
        return sb.ToString().Trim();
    }
    
    public static IEnumerable<T> DedupeBy<T, TKey>(
        IEnumerable<T> source,
        Func<T, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
    {
        var seen = new HashSet<TKey>(comparer ?? EqualityComparer<TKey>.Default);
        foreach (var item in source)
        {
            if (seen.Add(keySelector(item)))
                yield return item; // keep first occurrence
        }
    }
    
    public static string? NullIfWhiteSpace(string? s) => string.IsNullOrWhiteSpace(s) ? null : s!.Trim();
}