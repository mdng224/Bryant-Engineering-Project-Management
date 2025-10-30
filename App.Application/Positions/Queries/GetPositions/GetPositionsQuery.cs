using App.Application.Common.Abstractions;

namespace App.Application.Positions.Queries.GetPositions;

public sealed record GetPositionsQuery(int Page, int PageSize) : IPagedQuery;
