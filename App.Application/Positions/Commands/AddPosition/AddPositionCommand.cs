namespace App.Application.Positions.Commands.AddPosition;

public record AddPositionCommand(
    string Name,
    string Code,
    bool RequiresLicense);