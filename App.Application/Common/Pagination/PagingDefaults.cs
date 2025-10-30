namespace App.Application.Common.Pagination;

public static class PagingDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;

    /// <summary>
    /// Normalizes and clamps page and pageSize values.
    /// Ensures they fall within safe, consistent bounds.
    /// </summary>
    public static (int Page, int PageSize, int Skip) Normalize(int page, int pageSize)
    {
        var normalizedPage = page < 1 ? DefaultPage : page;
        var normalizedPageSize = pageSize < 1
            ? DefaultPageSize
            : Math.Min(pageSize, MaxPageSize);

        var skip = (normalizedPage - 1) * normalizedPageSize;

        return (normalizedPage, normalizedPageSize, skip);
    }
}