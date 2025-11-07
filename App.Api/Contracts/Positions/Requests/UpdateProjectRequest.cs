namespace App.Api.Contracts.Positions.Requests;

public sealed record UpdateProjectRequest(
    string Name,
    string Code,
    bool RequiresLicense);