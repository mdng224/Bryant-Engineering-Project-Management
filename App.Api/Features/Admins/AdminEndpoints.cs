using App.Api.Contracts.Admins;
using App.Api.Filters;
using App.Application.Abstractions;
using App.Application.Admins.Commands.UpdateUser;
using App.Application.Admins.Queries;
using App.Application.Common;

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
              .Produces<GetUsersResult>(StatusCodes.Status200OK)
              .Produces(StatusCodes.Status403Forbidden);

        // PUT /admins/users/{id}
        admins.MapPut("/users/{userId:guid}", UpdateUser)
              .AddEndpointFilter<Validate<UpdateUserRequest>>()
              .WithSummary("Update a user's role and/or active status")
              .Produces(StatusCodes.Status204NoContent)
              .Produces(StatusCodes.Status404NotFound)
              .ProducesValidationProblem(StatusCodes.Status400BadRequest)
              .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> GetUsers(
        [AsParameters] GetUsersQuery getUsersQuery,  // binds ?page=&pageSize=
        IQueryHandler<GetUsersQuery, Result<GetUsersResult>> getUsersHandler,
        CancellationToken ct)
    {
        var result = await getUsersHandler.Handle(getUsersQuery, ct);

        return Results.Ok(result.Value);
    }

    private static async Task<IResult> UpdateUser(
        Guid userId,
        UpdateUserRequest updateUserRequest,
        ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>> updateUserHandler,
        CancellationToken ct)
    {
        var updateUserCommand = new UpdateUserCommand(
            userId,
            updateUserRequest.RoleName,
            updateUserRequest.IsActive);
        var result = await updateUserHandler.Handle(updateUserCommand, ct);

        return result.Value switch
        {
            UpdateUserResult.Ok => Results.NoContent(),
            UpdateUserResult.UserNotFound => Results.NotFound(new { Message = "User not found." }),
            UpdateUserResult.NoChangesSpecified => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["Provide roleName and/or isActive."] }),

            _ => Results.Problem("Unknown error.")
        };
    }
}