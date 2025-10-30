using App.Application.Common.Pagination;

namespace App.Application.Positions.Queries.GetPositions;

public sealed record GetPositionsQuery(PagedQuery PagedQuery);
