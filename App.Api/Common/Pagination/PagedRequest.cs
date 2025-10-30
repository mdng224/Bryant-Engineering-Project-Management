namespace App.Api.Common.Pagination;

public sealed record PagedRequest(int Page = 1, int PageSize = 25);