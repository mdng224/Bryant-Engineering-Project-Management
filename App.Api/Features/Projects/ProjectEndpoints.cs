using App.Api.Contracts.Projects.Requests;
using App.Api.Contracts.Projects.Responses;
using App.Api.Features.Projects.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Projects.Commands.RestoreProject;
using App.Application.Projects.Queries.GetProjects;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects;

public static class ProjectEndpoints
{
    public static void MapProjectEndpoints(this IEndpointRouteBuilder app)
    {
        var projects = app.MapGroup("/projects")
            .WithTags("Projects");

        // GET /projects?page=&pageSize=
        projects.MapGet("", HandleGetProjects)
            .WithSummary("List all projects (paginated)")
            .Produces<GetProjectsResponse>();
        
        // POST /projects/{id}/restore
        projects.MapPost("/{id:guid}/restore", HandleRestoreProject)
            .WithSummary("Restore a soft-deleted project")
            .Produces<ProjectResponse>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleGetProjects(
        [AsParameters] GetProjectsRequest request,
        IQueryHandler<GetProjectsQuery, Result<PagedResult<ProjectDto>>> getProjectsHandler,
        CancellationToken ct = default)
    {
        var query   = request.ToQuery();
        var result  = await getProjectsHandler.Handle(query, ct);
        
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToGetProjectsResponse();

        return Ok(response);
    }
    
    private static async Task<IResult> HandleRestoreProject(
        [FromRoute] Guid id,
        ICommandHandler<RestoreProjectCommand, Result<ProjectDto>> handler,
        CancellationToken ct)
    {
        var command = new RestoreProjectCommand(id);
        var result  = await handler.Handle(command, ct);

        if (!result.IsSuccess)
        {
            var error = result.Error!.Value;
            return error.Code switch
            {
                "not_found" => NotFound(new { message = error.Message }),
                "conflict"  => Conflict(new { message = error.Message }),   // unique-name/code taken
                "forbidden" => TypedResults.Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
                _           => Problem(error.Message)
            };
        }

        var response = result.Value!.ToSummaryResponse();
        return Ok(response);
    }
}