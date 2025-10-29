namespace App.Application.Common.Dtos;

// For add and update position result
public record PositionResult(
    Guid Id,
    string Name,
    string? Code,
    bool RequiresLicense
);