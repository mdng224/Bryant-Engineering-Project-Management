namespace App.Api.Features.Clients.ListClientLookups;

public record ListClientLookupsResponse(
    IReadOnlyList<ClientCategoryResponse> Categories,
    IReadOnlyList<ClientTypeResponse> Types);