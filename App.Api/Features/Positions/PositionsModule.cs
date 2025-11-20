using App.Api.Features.Positions.AddPosition;
using App.Api.Features.Positions.DeletePosition;
using App.Api.Features.Positions.ListPositions;
using App.Api.Features.Positions.RestorePosition;
using App.Api.Features.Positions.UpdatePosition;

namespace App.Api.Features.Positions;

public static class PositionsModule
{
    public static void MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var positions = app.MapGroup("/positions")
            .WithTags("Positions");
        
        positions.MapAddPositionEndpoint();
        positions.MapDeletePositionEndpoint();
        positions.MapListPositionsEndpoint();
        positions.MapRestorePositionEndpoint();
        positions.MapUpdatePositionEndpoint();
    }
}