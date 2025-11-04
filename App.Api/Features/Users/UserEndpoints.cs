using App.Api.Contracts.Users;
using App.Api.Features.Users.Mappers;
using App.Api.Filters;
using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Users.Commands.DeleteUser;
using App.Application.Users.Commands.UpdateUser;
using App.Application.Users.Queries;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization("AdminOnly");

        // DELETE /positions/{id}
        users.MapDelete("/{id:guid}", HandleDeleteUser)
            .WithSummary("Delete a user")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        
        // GET /users?page=&pageSize=
        users.MapGet("", HandleGetUsers)
            .WithSummary("List users (paginated)")
            .Produces<GetUsersResponse>()
            .Produces(StatusCodes.Status403Forbidden);

        // PATCH /users/{id}
        users.MapPatch("/{userId:guid}", HandleUpdateUser)
            .AddEndpointFilter<Validate<UpdateUserRequest>>()
            .WithSummary("Update a user's role and/or status")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden);
    }
    
    private static async Task<IResult> HandleDeleteUser(
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
    
    private static async Task<IResult> HandleGetUsers(
        [AsParameters] GetUsersRequest request,
        IQueryHandler<GetUsersQuery, Result<PagedResult<UserDto>>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
        {
            var e = result.Error!.Value;
            return e.Code switch
            {
                "validation" => ValidationProblem(new Dictionary<string, string[]> { ["query"] = [e.Message] }),
                "forbidden"  => Json(new { message = e.Message }, statusCode: StatusCodes.Status403Forbidden),
                _            => Problem(e.Message)
            };
        }

        var response = result.Value!.ToResponse();
        return Ok(response);
    }
    
    private static async Task<IResult> HandleUpdateUser(
        [FromRoute] Guid userId,
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