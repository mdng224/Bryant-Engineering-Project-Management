using App.Api.Features.Clients.ListClientLookups;
using App.Application.Common.Dtos.Clients.Lookups;

namespace App.Api.Features.Clients.Mappers;

public static class ClientMappers
{
    public static IReadOnlyList<ClientCategoryResponse> ToResponses(this IReadOnlyList<ClientCategoryDto> dtos) =>
        dtos.Select(ccd => ccd.ToResponse()).ToList();
    
    public static IReadOnlyList<ClientTypeResponse> ToResponses(this IReadOnlyList<ClientTypeDto> dtos) =>
        dtos.Select(ccd => ccd.ToResponse()).ToList();
    
    // -- HELPERS --
    private static ClientCategoryResponse ToResponse(this ClientCategoryDto dto) => new(dto.Id, dto.Name);
    private static ClientTypeResponse ToResponse(this ClientTypeDto dto) =>
        new(dto.Id, dto.Name, dto.Description, dto.CategoryId);
}