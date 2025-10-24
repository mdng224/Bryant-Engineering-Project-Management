using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Positions.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Positions;

public static class PositionsModule
{
    public static IServiceCollection AddPositionsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetAllPositionsQuery, Result<GetAllPositionsResult>>, GetAllPositionsHandler>();

        // Commands

        return services;
    }
}