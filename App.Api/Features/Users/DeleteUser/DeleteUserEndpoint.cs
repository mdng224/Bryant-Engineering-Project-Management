using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Users.Commands.DeleteUser;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users.DeleteUser;

public static class DeleteUserEndpoint
{
    public static RouteGroupBuilder MapDeleteUserEndpoint(this RouteGroupBuilder group)
    {
        // DELETE /Users/{id}
        group.MapDelete("/{id:guid}", Handle)
            .WithSummary("Delete a user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        ICommandHandler<DeleteUserCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = new DeleteUserCommand(id);
        var result = await handler.Handle(command, ct);

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