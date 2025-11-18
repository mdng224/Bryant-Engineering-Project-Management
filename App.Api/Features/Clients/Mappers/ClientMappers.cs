using App.Api.Contracts.Clients.Requests;
using App.Api.Contracts.Clients.Responses;
using App.Api.Contracts.Clients.Responses.Lookups;
using App.Application.Clients.Commands.AddClient;
using App.Application.Clients.Queries.GetClients;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Application.Common.Pagination;
using App.Domain.Common;

namespace App.Api.Features.Clients.Mappers;

public static class ClientMappers
{
    public static AddClientCommand ToCommand(this AddClientRequest request) =>
        new(
            Name:             request.Name,
            NamePrefix:       request.NamePrefix,
            FirstName:        request.FirstName,
            MiddleName:       request.MiddleName,
            LastName:         request.LastName,
            NameSuffix:       request.NameSuffix,
            Email:            request.Email,
            Phone:            request.Phone,
            Address:          request.Address,
            Note:             request.Note,
            ClientCategoryId: request.ClientCategoryId,
            ClientTypeId:     request.ClientTypeId);
    
    public static GetClientsResponse ToResponse(this PagedResult<ClientListItemDto> pagedResult)
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
        var pagedQuery      = new PagedQuery(request.Page, request.PageSize);
        var getClientsQuery = new GetClientsQuery(
            pagedQuery,
            normalizedNameFilter,
            request.HasActiveProject,
            request.CategoryId,
            request.TypeId);

        return getClientsQuery;
    }

    public static IReadOnlyList<ClientCategoryResponse> ToResponses(this IReadOnlyList<ClientCategoryDto> dtos) =>
        dtos.Select(ccd => ccd.ToResponse()).ToList();
    
    public static IReadOnlyList<ClientTypeResponse> ToResponses(this IReadOnlyList<ClientTypeDto> dtos) =>
        dtos.Select(ccd => ccd.ToResponse()).ToList();
    
    // -- HELPERS --
    private static ClientCategoryResponse ToResponse(this ClientCategoryDto dto) => new(dto.Id, dto.Name);
    private static ClientTypeResponse ToResponse(this ClientTypeDto dto) =>
        new(dto.Id, dto.Name, dto.Description, dto.CategoryId);
    
    private static ClientSummaryResponse ToSummaryResponse(this ClientListItemDto dto) =>
        new(
            Id:                  dto.Id,
            Name:                dto.Name,
            TotalActiveProjects: dto.TotalActiveProjects,
            TotalProjects:       dto.TotalProjects,
            FirstName:           dto.FirstName,
            LastName:            dto.LastName,
            Email:               dto.Email,
            Phone:               dto.Phone,
            CategoryName:        dto.CategoryName,
            TypeName:            dto.TypeName);
    
    private static ClientListItemResponse ToListItem(this ClientListItemDto dto) =>
        new(ClientSummaryResponse: dto.ToSummaryResponse(),
            ClientDetailsResponse: dto.ToClientResponse());

    private static ClientDetailsResponse ToClientResponse(this ClientListItemDto dto) =>
        new(
            Id:                  dto.Id,
            Name:                dto.Name,
            TotalActiveProjects: dto.TotalActiveProjects,
            TotalProjects:       dto.TotalProjects,
            FirstName:           dto.FirstName,
            LastName:            dto.LastName,
            Email:               dto.Email,
            Phone:               dto.Phone,
            Address:             dto.Address,
            Note:                dto.Note,
            CategoryName:        dto.CategoryName,
            TypeName:            dto.TypeName,
            CreatedAtUtc:        dto.CreatedAtUtc,
            UpdatedAtUtc:        dto.UpdatedAtUtc,
            DeletedAtUtc:        dto.DeletedAtUtc,
            CreatedById:         dto.CreatedById,
            UpdatedById:         dto.UpdatedById,
            DeletedById:         dto.DeletedById);
}