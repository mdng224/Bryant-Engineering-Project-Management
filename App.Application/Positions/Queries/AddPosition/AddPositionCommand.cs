namespace App.Application.Positions.Queries.AddPosition;

public sealed record AddPositionCommand(
    string Name,
    string Code,
    bool RequiresLicense
);