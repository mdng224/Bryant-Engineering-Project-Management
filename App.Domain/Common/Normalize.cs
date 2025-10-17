namespace App.Domain.Common;

/// <summary>
/// Provides normalization and cleaning helpers for domain values (names, emails, phones, addresses, etc.).
/// </summary>
public static class Normalize
{
    // --- Names / general text ------------------------------------------------
    public static string ToNormalizedName(this string value)
        => CollapseSpaces(value.Trim()); // collapse OK for names
    public static string ToNormalizedEmail(this string value)
        => value.Trim().ToLowerInvariant();
    public static string? ToNormalizedPhone(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string([.. value.Where(char.IsDigit)]);

        return string.IsNullOrEmpty(digits) ? null : digits;
    }

    // --- Address-ish fields --------------------------------------------------
    public static string? ToNormalizedAddressLine(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim(); // NO collapse

    public static string? ToNormalizedCity(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : CollapseSpaces(value.Trim()); // collapse OK

    public static string? ToNormalizedState(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : CollapseSpaces(value.Trim()).ToUpperInvariant();

    public static string? ToNormalizedPostal(this string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var compact = new string([.. value.Where(c => !char.IsWhiteSpace(c) && c != '-')]);

        return compact.ToUpperInvariant();
    }

    public static string? ToNormalizedCountry(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : CollapseSpaces(value.Trim()); // collapse OK

    // --- Free-form -----------------------------------------------------------
    public static string? ToNormalizedNote(this string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // --- Private helpers -----------------------------------------------------
    private static string CollapseSpaces(this string s)
        => string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
}
