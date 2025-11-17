namespace App.Api.Contracts.Clients.Responses.Lookups;

public record ClientTypeResponse(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId);