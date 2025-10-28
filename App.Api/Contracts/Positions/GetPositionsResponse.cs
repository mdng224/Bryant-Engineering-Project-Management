namespace App.Api.Contracts.Positions;

public sealed record GetPositionsResponse(
    IReadOnlyList<PositionResponse> Positions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);