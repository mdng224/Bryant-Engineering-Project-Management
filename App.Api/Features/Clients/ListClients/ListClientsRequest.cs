namespace App.Api.Features.Clients.ListClients;

public record ListClientsRequest(
    int Page,
    int PageSize,
    string? NameFilter,
    bool HasActiveProject,
    Guid? CategoryId,
    Guid? TypeId
);