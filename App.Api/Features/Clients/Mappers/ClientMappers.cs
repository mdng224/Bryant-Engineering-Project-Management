using App.Api.Contracts.Clients.Requests;
using App.Api.Contracts.Clients.Responses;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Domain.Common;

namespace App.Api.Features.Clients.Mappers;

public static class ClientMappers
{
    public static GetClientsResponse ToGetClientsResponse(this PagedResult<ClientDto> pagedResult)
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
        var getClientsQuery           = new GetClientsQuery(pagedQuery, normalizedNameFilter, request.IsDeleted);

        return getClientsQuery;
    }
    
    public static ClientSummaryResponse ToSummaryResponse(this ClientDto dto) =>
        new(
            Id:            dto.Id,
            Name:          dto.Name,
            ContactFirst:  dto.ContactFirst,
            ContactMiddle: dto.ContactMiddle,
            ContactLast:   dto.ContactLast,
            Email:         dto.Email,
            Phone:         dto.Phone);
    
    private static ClientListItemResponse ToListItem(this ClientDto dto) =>
        new(
            Summary: dto.ToSummaryResponse(),
            Details: dto.ToClientResponse());

    private static ClientResponse ToClientResponse(this ClientDto dto) =>
        new(
            Id:            dto.Id,
            Name:          dto.Name,
            ContactFirst:  dto.ContactFirst,
            ContactMiddle: dto.ContactMiddle,
            ContactLast:   dto.ContactLast,
            Email:         dto.Email,
            Phone:         dto.Phone,
            Address:       dto.Address,
            Note:          dto.Note,
            CreatedAtUtc:  dto.CreatedAtUtc,
            UpdatedAtUtc:  dto.UpdatedAtUtc,
            DeletedAtUtc:  dto.DeletedAtUtc,
            CreatedById:   dto.CreatedById,
            UpdatedById:   dto.UpdatedById,
            DeletedById:   dto.DeletedById);
}