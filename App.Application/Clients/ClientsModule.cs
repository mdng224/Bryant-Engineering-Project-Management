using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries;
using App.Application.Common.Dtos;
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

        // Commands

        return services;
    }
}