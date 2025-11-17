namespace App.Api.Contracts.Clients.Responses.Lookups;

public record ClientLookupsResponse(
    IReadOnlyList<ClientCategoryResponse> Categories,
    IReadOnlyList<ClientTypeResponse> Types);