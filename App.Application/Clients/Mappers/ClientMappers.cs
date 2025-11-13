using App.Application.Common.Dtos;
using App.Domain.Clients;

namespace App.Application.Clients.Mappers;

public static class ClientMappers
{
    public static ClientListItemDto ToDto(this Client client, int totalActiveProjects, int totalProjects) =>
        new(
            Id:                  client.Id,
            Name:                client.Name,
            TotalActiveProjects: totalActiveProjects,
            TotalProjects:       totalProjects,
            FirstName:        client.FirstName,
            LastName:         client.LastName,
            Email:               client.Email,
            Phone:               client.Phone,
            Address:             client.Address,
            Note:                client.Note,
            CreatedAtUtc:        client.CreatedAtUtc,
            UpdatedAtUtc:        client.UpdatedAtUtc,
            DeletedAtUtc:        client.DeletedAtUtc,
            CreatedById:         client.CreatedById,
            UpdatedById:         client.UpdatedById,
            DeletedById:         client.DeletedById
            );
}