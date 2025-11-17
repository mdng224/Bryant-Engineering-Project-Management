using App.Application.Common.Pagination;

namespace App.Application.Clients.Queries.GetClients;

public sealed record GetClientsQuery(
    PagedQuery PagedQuery,
    string? NameFilter,
    bool HasActiveProject,
    Guid? CategoryId,
    Guid? TypeId);