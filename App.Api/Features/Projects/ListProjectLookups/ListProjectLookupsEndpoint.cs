using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Results;
using App.Application.Projects.Queries.GetProjectLookups;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects.ListProjectLookups;

public static class ListProjectLookupsEndpoint
{
    public static RouteGroupBuilder MapListProjectLookupsEndpoint(this RouteGroupBuilder group)
    {
        // GET /projects/lookups
        group.MapGet("/lookups", Handle)
            .WithSummary("Get lookup data for project managers")
            .Produces<ListProjectLookupsResponse>();

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromServices] IQueryHandler<GetProjectLookupsQuery, Result<ProjectLookupsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetProjectLookupsQuery(), ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = new ListProjectLookupsResponse(result.Value!.Managers);

        return Ok(response);
    }
}