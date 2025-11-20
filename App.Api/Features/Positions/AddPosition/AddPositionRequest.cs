namespace App.Api.Features.Positions.AddPosition;

public sealed record AddPositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);