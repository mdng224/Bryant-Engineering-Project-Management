namespace App.Api.Contracts.Positions.Requests;

public sealed record UpdatePositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);