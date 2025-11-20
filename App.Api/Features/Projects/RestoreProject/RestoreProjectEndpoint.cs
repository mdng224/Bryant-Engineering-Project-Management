using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Projects.Commands.RestoreProject;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Projects.RestoreProject;

public static class RestoreProjectEndpoint
{
    public static RouteGroupBuilder MapRestoreProjectEndpoint(this RouteGroupBuilder group)
    {
        // POST /projects/{id}/restore
        group.MapPost("/{id:guid}/restore", Handle)
            .WithSummary("Restore a soft-deleted project")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestoreProjectCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = new RestoreProjectCommand(id);
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent(); // ✔ correct REST behavior

        var error = result.Error!.Value;

        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "conflict"  => Conflict(new { message = error.Message }),
            "forbidden" => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _           => Problem(error.Message)
        };
    }
}