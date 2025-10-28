using App.Application.Common.Dtos;

namespace App.Application.Positions.Queries.GetPositions;

public sealed record GetPositionsResult(
    IReadOnlyList<PositionDto> Positions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);