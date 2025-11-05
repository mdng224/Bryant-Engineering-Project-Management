namespace App.Application.Common.Dtos;

public sealed record PositionDto(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense,
    DateTimeOffset? DeletedAtUtc
);