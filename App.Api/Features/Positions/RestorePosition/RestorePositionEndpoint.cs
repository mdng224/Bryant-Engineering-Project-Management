using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Positions.Commands.RestorePosition;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions.RestorePosition;

public static class RestorePositionEndpoint
{
    public static RouteGroupBuilder MapRestorePositionEndpoint(this RouteGroupBuilder group)
    {
        // POST /positions/{id}/restore
        group.MapPost("/{id:guid}/restore", Handle)
            .WithSummary("Restore a soft-deleted position")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestorePositionCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = new RestorePositionCommand(id);
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();
        
        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "conflict"  => Conflict(new { message = error.Message }),   // unique-name/code taken
            "forbidden" => TypedResults.Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _           => Problem(error.Message)
        };
    }
}