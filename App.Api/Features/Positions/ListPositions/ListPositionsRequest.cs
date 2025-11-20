namespace App.Api.Features.Positions.ListPositions;

public sealed record ListPositionsRequest(
    int Page,
    int PageSize,
    string? NameFilter,
    bool? IsDeleted
);