namespace App.Api.Contracts.Positions.Requests;

public sealed record AddPositionRequest(
    string Name,
    string Code,
    bool RequiresLicense);