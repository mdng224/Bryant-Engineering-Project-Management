namespace App.Application.Common.Dtos;

// For read
public sealed record PositionDto(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense
);