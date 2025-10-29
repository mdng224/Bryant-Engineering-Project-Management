using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.DeletePosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries;
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
        services.AddScoped<ICommandHandler<AddPositionCommand, Result<PositionResult>>, AddPositionHandler>();
        services.AddScoped<ICommandHandler<DeletePositionCommand, Result<Unit>>, DeletePositionHandler>();
        services.AddScoped<ICommandHandler<UpdatePositionCommand, Result<PositionResult>>, UpdatePositionHandler>();
        
        return services;
    }
}