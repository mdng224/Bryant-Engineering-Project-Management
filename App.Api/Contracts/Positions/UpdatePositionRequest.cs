namespace App.Api.Contracts.Positions;

public sealed record UpdatePositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);