namespace App.Api.Contracts.Positions;

// For adds/updates
public record PositionResponse(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc);