using App.Api.Contracts.Admins;
using App.Api.Filters;
using App.Api.Mappers.Admins;
using App.Application.Abstractions;
using App.Application.Admins.Commands.UpdateUser;
using App.Application.Admins.Queries;
using App.Application.Common;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Admins;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admins = app.MapGroup("/admins")
            .WithTags("Admins")
            .RequireAuthorization("AdminOnly");

        // GET /admins/users?page=&pageSize=
        admins.MapGet("/users", GetUsers)
            .WithSummary("List users (paginated)")
            .Produces<GetUsersResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status403Forbidden);

        // PUT /admins/users/{id}
        admins.MapPatch("/users/{userId:guid}", UpdateUser)
            .AddEndpointFilter<Validate<UpdateUserRequest>>()
            .WithSummary("Update a user's role and/or active status")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] GetUsersRequest request,  // binds ?page=&pageSize=
        IQueryHandler<GetUsersQuery, Result<GetUsersResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message ?? "Unexpected error.");

        var response = result.Value!.ToResponse();

        return Ok(response);
    }

    private static async Task<IResult> UpdateUser(
        Guid userId,
        UpdateUserRequest request,
        ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand(userId);
        var result = await handler.Handle(command, ct);

        return !result.IsSuccess ? ToHttpError(result.Error!.Value) : ToHttpSuccess(result.Value);
    }
    
    private static IResult ToHttpError(Error error)
    {
        return error.Code switch
        {
            "not_found"  => NotFound(new { message = error.Message }),
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            "conflict"   => Conflict(new { message = error.Message }),
            "validation" => ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = [error.Message ?? "Validation failed."] }),
            _            => Problem(error.Message ?? "Unexpected error.")
        };
    }

    private static IResult ToHttpSuccess(UpdateUserResult value)
    {
        return value switch
        {
            UpdateUserResult.Ok                 => NoContent(),
            UpdateUserResult.UserNotFound       => NotFound(new { message = "User not found." }),
            UpdateUserResult.RoleNotFound       => NotFound(new { message = "Role not found." }),
            UpdateUserResult.NoChangesSpecified => ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["Provide roleName and/or isActive."] }),
            _ => Problem("Unknown result.")
        };
    }

}