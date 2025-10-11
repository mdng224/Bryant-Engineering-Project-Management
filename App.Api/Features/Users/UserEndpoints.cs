using App.Api.Contracts.Users;
using App.Api.Filters;
using App.Application.Abstractions;
using App.Application.Users;

namespace App.Api.Features.Users;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users").WithTags("Users");

        // PUT /users/{userId}/role
        users.MapPut("/{userId:guid}/role", SetUserRole)
             .RequireAuthorization("AdminOnly")
             .AddEndpointFilter<Validate<SetUserRoleRequest>>()   // runs FluentValidation
             .Accepts<SetUserRoleRequest>("application/json")
             .WithName("Users_SetUserRole")
             .WithSummary("Set a user's role")
             .WithDescription("Replaces the user's role with the provided role name.")
             .Produces(StatusCodes.Status204NoContent)
             .Produces(StatusCodes.Status404NotFound)
             .ProducesValidationProblem(StatusCodes.Status400BadRequest)
             .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> SetUserRole(
        Guid userId,
        SetUserRoleRequest body,
        IUserService userService,
        CancellationToken ct)
    {
        var result = await userService.SetUserRoleAsync(userId, body.RoleName, ct);
        var outcome = result.Value;

        return outcome switch
        {
            SetUserRoleResult.Ok => Results.NoContent(),
            SetUserRoleResult.UserNotFound => Results.NotFound(new { Message = "User not found." }),
            SetUserRoleResult.RoleNotFound => Results.NotFound(new { Message = $"Role '{body.RoleName}' not found." }),
            _ => Results.ValidationProblem(
                    new Dictionary<string, string[]>
                    {
                        ["roleName"] = ["Invalid role name."]
                    })
        };
    }
}
