using App.Api.Contracts.Positions;

namespace App.Api.Features.Positions;

public static class PositionEndpoints
{
    public static void MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/positions")
            .WithTags("positions");

        // GET /positions?page=&pageSize=
        users.MapGet("/", GetPositions.Handle)
            .WithSummary("List positions (paginated)")
            .Produces<GetPositionsResponse>();
    }
}