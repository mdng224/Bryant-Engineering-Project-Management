namespace App.Application.Common.Dtos.Positions;

public sealed record PositionListItemDto(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc
);