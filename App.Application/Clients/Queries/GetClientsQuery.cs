using App.Application.Common.Pagination;

namespace App.Application.Clients.Queries;

public sealed record GetClientsQuery(PagedQuery PagedQuery, string? NameFilter);