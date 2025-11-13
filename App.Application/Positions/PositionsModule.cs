using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.DeletePosition;
using App.Application.Positions.Commands.RestorePosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries.GetPositions;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Positions;

public static class PositionsModule
{
    public static IServiceCollection AddPositionsApplication(this IServiceCollection services)
    {
        // Queries
        services.AddScoped<IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionListItemDto>>>, GetPositionsHandler>();

        // Commands
        services.AddScoped<ICommandHandler<AddPositionCommand, Result<Unit>>, AddPositionHandler>();
        services.AddScoped<ICommandHandler<DeletePositionCommand, Result<Unit>>, DeletePositionHandler>();
        services.AddScoped<ICommandHandler<UpdatePositionCommand, Result<Unit>>, UpdatePositionHandler>();
        services.AddScoped<ICommandHandler<RestorePositionCommand, Result<Unit>>, RestorePositionHandler>();
        
        return services;
    }
}