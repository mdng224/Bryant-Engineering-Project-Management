using App.Api.Contracts.Projects.Responses;
using App.Api.Features.Projects.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Projects;
using App.Application.Common.Results;
using App.Application.Projects.Queries.GetProjectLookups;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects;

public static class ProjectLookupEndpoints
{
    public static void MapProjectLookupEndpoints(this IEndpointRouteBuilder app)
    {
        var lookups = app.MapGroup("/projects/lookups")
            .WithTags("Project lookups");
        
        // GET /projects/lookups
        lookups.MapGet("", HandleGetProjectLookups)
            .WithSummary("Get lookup data for project managers")
            .Produces<ProjectLookupsResponse>();
    }
    
    private static async Task<IResult> HandleGetProjectLookups(
        [FromServices] IQueryHandler<GetProjectLookupsQuery, Result<ProjectLookupsDto>> handler,
        CancellationToken ct)
    {
        var result = await handler.Handle(new GetProjectLookupsQuery(), ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var dto = result.Value!;

        var response = dto.ToResponse();

        return Ok(response);
    }
}