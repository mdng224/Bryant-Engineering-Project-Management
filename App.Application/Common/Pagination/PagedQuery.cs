namespace App.Application.Common.Pagination;

public sealed record PagedQuery
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 25;
    private const int MaxPageSize = 100;

    public int Page { get; }
    public int PageSize { get; }
    public int Skip => (Page - 1) * PageSize;

    public PagedQuery(int page, int pageSize)
    {
        Page = page <= 0 ? DefaultPage : page;
        PageSize = pageSize <= 0 ? DefaultPageSize
            : pageSize > MaxPageSize ? MaxPageSize
            : pageSize;
    }

    public void Deconstruct(out int page, out int pageSize, out int skip)
    {
        page = Page;
        pageSize = PageSize;
        skip = Skip;
    }
}