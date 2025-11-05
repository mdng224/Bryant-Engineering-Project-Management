namespace App.Api.Contracts.Positions.Responses;

public sealed record GetPositionsResponse(
    IReadOnlyList<PositionResponse> Positions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);