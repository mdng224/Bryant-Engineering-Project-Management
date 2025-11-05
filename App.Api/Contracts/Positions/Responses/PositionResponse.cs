namespace App.Api.Contracts.Positions.Responses;

// For adds/updates
public record PositionResponse(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc);