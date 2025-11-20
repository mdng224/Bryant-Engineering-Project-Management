namespace App.Api.Features.Positions.ListPositions;

public sealed record ListPositionsResponse(
    IReadOnlyList<PositionRowResponse> Positions,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);