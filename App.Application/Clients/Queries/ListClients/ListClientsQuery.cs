using App.Application.Common.Pagination;

namespace App.Application.Clients.Queries.ListClients;

public sealed record ListClientsQuery(
    PagedQuery PagedQuery,
    string? NameFilter,
    bool HasActiveProject,
    Guid? CategoryId,
    Guid? TypeId
);