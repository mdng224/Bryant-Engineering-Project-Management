using App.Application.Common.Dtos;
using App.Domain.Clients;
using App.Domain.Employees;

namespace App.Application.Clients.Mappers;

public static class ClientMappers
{
    public static ClientDto ToDto(this Client client) => new ClientDto();
}