using App.Api.Features.Users.DeleteUser;
using App.Api.Features.Users.ListUsers;
using App.Api.Features.Users.RestoreUser;
using App.Api.Features.Users.UpdateUser;

namespace App.Api.Features.Users;

public static class UserModule
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var users = app.MapGroup("/users")
            .WithTags("Users")
            .RequireAuthorization("AdminOnly");

        users.MapDeleteUserEndpoint();
        users.MapListUsersEndpoint();
        users.MapRestoreUserEndpoint();
        users.MapUpdateUserEndpoint();
    }
}