using App.Api.Contracts.Clients.Requests;
using App.Api.Contracts.Clients.Responses;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Domain.Common;

namespace App.Api.Features.Clients.Mappers;

public static class ClientMappers
{
    public static GetClientsResponse ToGetClientsResponse(this PagedResult<ClientListItemDto> pagedResult)
    {
        var clientListItemResponse = pagedResult.Items
            .Select(cd => cd.ToListItem())
            .ToList();
        
        return new GetClientsResponse(
            ClientListItemResponses: clientListItemResponse,
            TotalCount:              pagedResult.TotalCount,
            Page:                    pagedResult.Page,
            PageSize:                pagedResult.PageSize,
            TotalPages:              pagedResult.TotalPages
        );
    }
    
    public static GetClientsQuery ToQuery(this GetClientsRequest request)
    {
        var normalizedNameFilter = (request.NameFilter ?? string.Empty).ToNormalizedName();
        var pagedQuery                = new PagedQuery(request.Page, request.PageSize);
        var getClientsQuery           = new GetClientsQuery(pagedQuery, normalizedNameFilter, request.HasActiveProject);

        return getClientsQuery;
    }
    
    public static ClientSummaryResponse ToSummaryResponse(this ClientListItemDto listItemDto) =>
        new(
            Id:                  listItemDto.Id,
            Name:                listItemDto.Name,
            TotalActiveProjects: listItemDto.TotalActiveProjects,
            TotalProjects:       listItemDto.TotalProjects,
            FirstName:        listItemDto.FirstName,
            LastName:         listItemDto.LastName,
            Email:               listItemDto.Email,
            Phone:               listItemDto.Phone);
    
    private static ClientListItemResponse ToListItem(this ClientListItemDto listItemDto) =>
        new(
            Summary: listItemDto.ToSummaryResponse(),
            Details: listItemDto.ToClientResponse());

    private static ClientResponse ToClientResponse(this ClientListItemDto listItemDto) =>
        new(
            Id:                  listItemDto.Id,
            Name:                listItemDto.Name,
            TotalActiveProjects: listItemDto.TotalActiveProjects,
            TotalProjects:       listItemDto.TotalProjects,
            FirstName:        listItemDto.FirstName,
            LastName:         listItemDto.LastName,
            Email:               listItemDto.Email,
            Phone:               listItemDto.Phone,
            Address:             listItemDto.Address,
            Note:                listItemDto.Note,
            CreatedAtUtc:        listItemDto.CreatedAtUtc,
            UpdatedAtUtc:        listItemDto.UpdatedAtUtc,
            DeletedAtUtc:        listItemDto.DeletedAtUtc,
            CreatedById:         listItemDto.CreatedById,
            UpdatedById:         listItemDto.UpdatedById,
            DeletedById:         listItemDto.DeletedById);
}