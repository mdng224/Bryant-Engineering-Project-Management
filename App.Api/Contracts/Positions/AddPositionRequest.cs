namespace App.Api.Contracts.Positions;

public sealed record AddPositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);