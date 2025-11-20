using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Users.Commands.RestoreUser;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users.RestoreUser;

public static class RestoreUserEndpoint
{
    public static RouteGroupBuilder MapRestoreUserEndpoint(this RouteGroupBuilder group)
    {
        // POST /Users/{id}/restore
        group.MapPost("/{id:guid}/restore", HandleRestoreUser)
            .WithSummary("Restore a soft-deleted User")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return group;
    }
    
    private static async Task<IResult> HandleRestoreUser(
        [FromRoute] Guid id,
        ICommandHandler<RestoreUserCommand, Result<UserDto>> handler,
        CancellationToken ct)
    {
        var command = new RestoreUserCommand(id);
        var result = await handler.Handle(command, ct);
        
        if (result.IsSuccess)
            return NoContent();
        
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