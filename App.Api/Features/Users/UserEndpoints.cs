using App.Api.Contracts.Users;
using App.Api.Filters;

namespace App.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("users")
            .RequireAuthorization("AdminOnly");

        // GET /users?page=&pageSize=
        users.MapGet("/", GetUsers.Handle)
            .WithSummary("List users (paginated)")
            .Produces<GetUsersResponse>()
            .Produces(StatusCodes.Status403Forbidden);

        // PATCH /users/users/{id}
        users.MapPatch("/{userId:guid}", UpdateUser.Handle)
            .AddEndpointFilter<Validate<UpdateUserRequest>>()
            .WithSummary("Update a user's role and/or status")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status403Forbidden);
    }
}