namespace App.Application.Positions.Commands.AddPosition;

public sealed record AddPositionCommand(
    string Name,
    string Code,
    bool RequiresLicense);