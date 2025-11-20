using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Users.Commands.UpdateUser;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users.UpdateUser;

public static class UpdateUserEndpoint
{
    public static RouteGroupBuilder MapUpdateUserEndpoint(this RouteGroupBuilder group)
    {
        // PATCH /users/{id}
        group.MapPatch("/{id:guid}", Handle)
            .AddEndpointFilter<Validate<UpdateUserRequest>>()
            .WithSummary("Update a user's role and/or status")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)      // validation
            .Produces(StatusCodes.Status403Forbidden)       // last-admin guard, authz policy
            .Produces(StatusCodes.Status404NotFound)        // user not found
            .Produces(StatusCodes.Status409Conflict);       // uniqueness / concurrency

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdateUserRequest request,
        ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>> handler,
        CancellationToken ct)
    {
        var command = new UpdateUserCommand(id, request.RoleName, request.Status);
        var result = await handler.Handle(command, ct);

        return !result.IsSuccess ? ToHttpError(result.Error!.Value) : ToHttpSuccess(result.Value);
    }
    
    private static IResult ToHttpSuccess(UpdateUserResult value) =>
        value switch
        {
            UpdateUserResult.Ok                 => NoContent(),
            UpdateUserResult.NoChangesSpecified => ValidationProblem(
                errors: new Dictionary<string, string[]> { ["body"] = ["Provide roleName and/or status."] }
            ),
            _ => Problem("Unknown result.")
        };

    private static IResult ToHttpError(Error error) =>
        error.Code switch
        {
            "not_found"  => NotFound(new { message = error.Message }),
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            "conflict"   => Conflict(new { message = error.Message }),
            "validation" => ValidationProblem(new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            _            => Problem(error.Message)
        };
}