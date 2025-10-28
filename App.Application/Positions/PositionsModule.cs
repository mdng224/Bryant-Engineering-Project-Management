using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Positions.Queries;
using App.Application.Positions.Queries.AddPosition;
using App.Application.Positions.Queries.GetPositions;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Positions;

public static class PositionsModule
{
    public static IServiceCollection AddPositionsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>>, GetPositionsHandler>();

        // Commands
        services.AddScoped<ICommandHandler<AddPositionCommand, Result<AddPositionResult>>, AddPositionHandler>();

        return services;
    }
}