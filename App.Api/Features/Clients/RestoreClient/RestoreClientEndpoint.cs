using App.Application.Abstractions.Handlers;
using App.Application.Clients.Commands.RestoreClient;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Clients.RestoreClient;

public static class RestoreClientEndpoint
{
    public static RouteGroupBuilder MapRestoreClientEndpoint(this RouteGroupBuilder group)
    {
        // POST /clients/{id}/restore
        group.MapPost("/{id:guid}/restore", Handle)
            .WithSummary("Restore a soft-deleted client")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestoreClientCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = new RestoreClientCommand(id);
        var result = await handler.Handle(command, ct);

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