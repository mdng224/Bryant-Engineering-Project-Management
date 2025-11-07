namespace App.Api.Contracts.Positions.Requests;

public sealed record AddProjectRequest(
    string Name,
    string Code,
    bool RequiresLicense);