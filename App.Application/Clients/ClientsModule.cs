using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.AddClient;
using App.Application.Clients.Queries;
using App.Application.Clients.Queries.GetClientDetails;
using App.Application.Clients.Queries.GetClientLookups;
using App.Application.Clients.Queries.ListClients;
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
        services.AddScoped<IQueryHandler<GetClientDetailsQuery, Result<ClientDetailsDto>>, GetClientDetailsHandler>();
        services.AddScoped<IQueryHandler<ListClientsQuery, Result<PagedResult<ClientRowDto>>>, ListClientsHandler>();
        services.AddScoped<IQueryHandler<GetClientLookupsQuery, Result<ClientLookupsDto>>, GetClientLookupsHandler>();

        // Commands
        services.AddScoped<ICommandHandler<AddClientCommand, Result<Guid>>, AddClientHandler>();

        return services;
    }
}