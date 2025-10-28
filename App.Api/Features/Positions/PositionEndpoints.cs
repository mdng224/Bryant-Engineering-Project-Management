using App.Api.Contracts.Positions;
using App.Api.Filters;

namespace App.Api.Features.Positions;

public static class PositionEndpoints
{
    public static void MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var positions = app.MapGroup("/positions")
            .WithTags("positions");

        // GET /positions?page=&pageSize=
        positions.MapGet("/", GetPositions.Handle)
            .WithSummary("List positions (paginated)")
            .Produces<GetPositionsResponse>();
        
        // POST /positions
        positions.MapPost("/", AddPosition.Handle)
            .AddEndpointFilter<Validate<AddPositionRequest>>()
            .WithSummary("Create a new position")
            .Accepts<AddPositionRequest>("application/json")
            .Produces<AddPositionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
    }
}