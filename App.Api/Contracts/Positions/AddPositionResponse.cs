namespace App.Api.Contracts.Positions;

public sealed record AddPositionResponse(
    Guid Id,
    string Name,
    string Code,
    bool RequiresLicense);