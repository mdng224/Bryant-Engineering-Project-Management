using App.Api.Contracts.Admins;
using App.Api.Filters;
using App.Application.Abstractions;
using App.Application.Admins;

namespace App.Api.Features.Admins;

public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admins = app.MapGroup("/admins")
                        .WithTags("Admins")
                        .RequireAuthorization("AdminOnly");

        // PUT /admins/users/{id}
        admins.MapPut("/users/{userId:guid}", UpdateUser)
              .AddEndpointFilter<Validate<AdminUpdateUserRequest>>()
              .WithSummary("Update a user's role and/or active status")
              .Produces(StatusCodes.Status204NoContent)
              .Produces(StatusCodes.Status404NotFound)
              .ProducesValidationProblem(StatusCodes.Status400BadRequest)
              .Produces(StatusCodes.Status403Forbidden);
    }

    private static async Task<IResult> UpdateUser(
        Guid userId,
        AdminUpdateUserRequest adminUpdateUserRequest,
        IAdminService adminService,
        CancellationToken ct)
    {
        var result = await adminService.UpdateUserAsync(
            userId,
            adminUpdateUserRequest.RoleName,
            adminUpdateUserRequest.IsActive,
            ct);

        return result.Value switch
        {
            AdminUpdateResult.Ok => Results.NoContent(),
            AdminUpdateResult.UserNotFound => Results.NotFound(new { Message = "User not found." }),
            AdminUpdateResult.NoChangesSpecified => Results.ValidationProblem(
                new Dictionary<string, string[]> { ["body"] = ["Provide roleName and/or isActive."] }),

            _ => Results.Problem("Unknown error.")
        };
    }
}