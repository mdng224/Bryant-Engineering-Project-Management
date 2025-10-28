namespace App.Application.Positions.Queries.AddPosition;

public sealed record AddPositionResult(
    Guid Id,
    string Name,
    string Code,
    bool RequiresLicense);