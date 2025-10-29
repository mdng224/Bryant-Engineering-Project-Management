namespace App.Application.Positions.Commands.UpdatePosition;

public sealed record UpdatePositionCommand(
    Guid PositionId,
    string Name,
    string Code,
    bool RequiresLicense);