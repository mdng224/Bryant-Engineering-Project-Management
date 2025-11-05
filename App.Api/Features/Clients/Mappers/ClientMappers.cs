using App.Api.Contracts.Clients.Requests;
using App.Api.Contracts.Clients.Responses;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Domain.Common;

namespace App.Api.Features.Clients.Mappers;

public static class ClientMappers
{
    public static GetClientsQuery ToQuery(this GetClientsRequest request)
    {
        var normalizedNameFilter = (request.NameFilter ?? string.Empty).ToNormalizedName();
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var getClientsQuery = new GetClientsQuery(pagedQuery, normalizedNameFilter);

        return getClientsQuery;
    }
    
    public static ClientResponse ToResponse(this ClientDto dto) =>
        new();
}