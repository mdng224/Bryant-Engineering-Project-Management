using App.Api.Common.Pagination;

namespace App.Api.Contracts.Positions;

public sealed record GetPositionsRequest(PagedRequest PagedRequest);