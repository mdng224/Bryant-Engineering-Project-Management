namespace App.Api.Contracts.Clients.Responses.Lookups;

public sealed record ClientCategoryResponse(
    Guid Id,
    string Name);