namespace App.Api.Features.Projects.ListProjectLookups;

public sealed record ListProjectLookupsResponse(IReadOnlyList<string> Managers);