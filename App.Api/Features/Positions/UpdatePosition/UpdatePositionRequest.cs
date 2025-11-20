namespace App.Api.Features.Positions.UpdatePosition;

public sealed record UpdatePositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);