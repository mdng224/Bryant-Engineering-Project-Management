namespace App.Api.Features.Positions.ListPositions;

// For adds/updates
public record PositionRowResponse(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc);