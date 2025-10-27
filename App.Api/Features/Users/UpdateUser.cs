using App.Api.Contracts.Users;
using App.Api.Mappers.Users;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Users.Commands.UpdateUser;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users;

public static class UpdateUser
{
    public static async Task<IResult> Handle(
        Guid userId,
        UpdateUserRequest request,
        ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand(userId);
        var result = await handler.Handle(command, ct);

        return !result.IsSuccess ? ToHttpError(result.Error!.Value) : ToHttpSuccess(result.Value);
    }
    
    private static IResult ToHttpSuccess(UpdateUserResult value) =>
        value switch
        {
            UpdateUserResult.Ok                 => NoContent(),
            UpdateUserResult.UserNotFound       => NotFound(new { message = "User not found." }),
            UpdateUserResult.RoleNotFound       => NotFound(new { message = "Role not found." }),
            UpdateUserResult.NoChangesSpecified => ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["Provide roleName and/or status."] }), // <- was isActive
            _ => Problem("Unknown result.")
        };

    private static IResult ToHttpError(Error error) =>
        error.Code switch
        {
            "not_found"  => NotFound(new { message = error.Message }),
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden), // ok
            "conflict"   => Conflict(new { message = error.Message }),
            "validation" => ValidationProblem(new Dictionary<string, string[]> { ["body"] = [error.Message ?? "Validation failed."] }),
            _            => Problem(error.Message)
        };

}