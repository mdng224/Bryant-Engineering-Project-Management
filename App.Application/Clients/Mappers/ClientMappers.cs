using App.Application.Common.Dtos;
using App.Domain.Clients;

namespace App.Application.Clients.Mappers;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) =>
        new(
            Id: client.Id,
            Name: client.Name,
            ContactFirst:  client.ContactFirst,
            ContactMiddle:  client.ContactMiddle,
            ContactLast:  client.ContactLast,
            Email:   client.Email,
            Phone:   client.Phone,
            Address:   client.Address,
            Note:   client.Note,
            CreatedAtUtc:  client.CreatedAtUtc,
            UpdatedAtUtc:  client.UpdatedAtUtc,
            DeletedAtUtc:  client.DeletedAtUtc,
            CreatedById: client.CreatedById,
            UpdatedById: client.UpdatedById,
            DeletedById: client.DeletedById
            );
}