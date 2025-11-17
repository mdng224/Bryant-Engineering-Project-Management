using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.AddClient;
using App.Application.Clients.Queries;
using App.Application.Clients.Queries.GetClientLookups;
using App.Application.Clients.Queries.GetClients;
using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Clients;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Clients;

public static class ClientsModule
{
    public static IServiceCollection AddClientsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetClientsQuery, Result<PagedResult<ClientListItemDto>>>, GetClientsHandler>();
        services.AddScoped<IQueryHandler<GetClientLookupsQuery, Result<ClientLookupsDto>>, GetClientLookupsHandler>();

        // Commands
        services.AddScoped<ICommandHandler<AddClientCommand, Result<Guid>>, AddClientHandler>();

        return services;
    }
}