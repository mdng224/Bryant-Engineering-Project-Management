using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Positions;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.DeletePosition;
using App.Application.Positions.Commands.RestorePosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries.GetPositions;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace App.Application.Positions;

public static class PositionsModule
{
    public static IServiceCollection AddPositionsApplication(this IServiceCollection services)
    {
        // Commands
        services.AddScoped<ICommandHandler<AddPositionCommand, Result<Guid>>, AddPositionHandler>();
        services.AddScoped<ICommandHandler<DeletePositionCommand, Result<Unit>>, DeletePositionHandler>();
        services.AddScoped<ICommandHandler<UpdatePositionCommand, Result<Unit>>, UpdatePositionHandler>();
        services.AddScoped<ICommandHandler<RestorePositionCommand, Result<Unit>>, RestorePositionHandler>();
        
        // Queries
        services.AddScoped<IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionListItemDto>>>, GetPositionsHandler>();

        // Validators
        services.AddScoped<IValidator<AddPositionCommand>, AddPositionCommandValidator>();
        services.AddScoped<IValidator<UpdatePositionCommand>, UpdatePositionCommandValidator>();
        
        return services;
    }
}