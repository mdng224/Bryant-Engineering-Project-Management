namespace App.Application.Common.Dtos;

public sealed record PositionListItemDto(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc
);