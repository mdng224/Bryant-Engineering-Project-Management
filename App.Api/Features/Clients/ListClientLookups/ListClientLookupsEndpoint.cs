using App.Application.Abstractions.Handlers;
using App.Application.Clients.Queries.GetClientLookups;
using App.Application.Common.Dtos.Clients.Lookups;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients.ListClientLookups;

public static class ListClientLookupsEndpoint
{
    public static RouteGroupBuilder MapListClientLookupsEndpoint(this RouteGroupBuilder group)
    {
        // GET /projects/lookups
        group.MapGet("/lookups", Handle)
            .WithSummary("Get lookup data for client category and client type")
            .Produces<ListClientLookupsResponse>();

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromServices] IQueryHandler<GetClientLookupsQuery, Result<ClientLookupsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetClientLookupsQuery(), ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var dto = result.Value!;
        var response = new ClientLookupsDto(dto.Categories, dto.Types);

        return Ok(response);
    }
}