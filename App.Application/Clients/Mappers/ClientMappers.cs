using App.Application.Clients.Commands.AddClient;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Domain.Clients;

namespace App.Application.Clients.Mappers;

public static class ClientMappers
{
    public static Client ToDomain(this AddClientCommand command) =>
        new(
            name: command.Name,
            categoryId: command.ClientCategoryId,
            typeId: command.ClientTypeId,
            projectCode: null
        );

    public static IReadOnlyList<ClientCategoryDto> ToDtos(this IReadOnlyList<ClientCategory> clientCategories) =>
        clientCategories.Select(cc => cc.ToDto()).ToList();
    
    public static IReadOnlyList<ClientTypeDto> ToDtos(this IReadOnlyList<ClientType> clientTypes) =>
        clientTypes.Select(ct => ct.ToDto()).ToList();

    private static ClientCategoryDto ToDto(this ClientCategory clientCategory) =>
        new(clientCategory.Id, clientCategory.Name);
    
    private static ClientTypeDto ToDto(this ClientType clientType) =>
        new(
            clientType.Id,
            clientType.Name,
            clientType.Description,
            clientType.CategoryId);
}