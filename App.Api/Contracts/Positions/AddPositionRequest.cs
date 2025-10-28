namespace App.Api.Contracts.Positions;

public record AddPositionRequest(string Name, string Code, bool RequiresLicense);