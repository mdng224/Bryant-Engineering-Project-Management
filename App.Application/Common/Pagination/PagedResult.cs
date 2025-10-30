namespace App.Application.Common.Pagination;

public sealed class PagedResult<T>(
    IReadOnlyList<T> items,
    int totalCount,
    int page,
    int pageSize)
{
    public IReadOnlyList<T> Items { get; } = items;
    public int TotalCount { get; } = totalCount;
    public int Page { get; } = page;
    public int PageSize { get; } = pageSize;

    public int TotalPages { get; } = pageSize <= 0
        ? 0
        : (int)Math.Ceiling(totalCount / (double)pageSize);

    // ✅ Inline mapper utility
    public PagedResult<TOut> Map<TOut>(Func<T, TOut> map) =>
        new(Items.Select(map).ToList(), TotalCount, Page, PageSize);
}