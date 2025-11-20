using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Positions.Commands.DeletePosition;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions.DeletePosition;

public static class DeletePositionEndpoint
{
    public static RouteGroupBuilder MapDeletePositionEndpoint(this RouteGroupBuilder group)
    {
        // DELETE /positions/{id}
        group.MapDelete("/{id:guid}", Handle)
            .WithSummary("Delete a position")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<DeletePositionCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = new DeletePositionCommand(id);
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();
        
        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "forbidden" => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _ => Problem(error.Message)
        };
    }
}