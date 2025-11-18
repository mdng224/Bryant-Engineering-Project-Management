namespace App.Api.Contracts.Projects.Responses;

public sealed record ProjectLookupsResponse(IReadOnlyList<string> Managers);