namespace App.Api.Contracts.Positions;

public record PositionResponse(Guid Id, string Name, string? Code, bool RequiresLicense);