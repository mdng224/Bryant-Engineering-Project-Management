namespace App.Api.Features.Clients.ListClientLookups;

public record ClientTypeResponse(
    Guid Id,
    string Name,
    string Description,
    Guid CategoryId
);