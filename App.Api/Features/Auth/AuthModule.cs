using App.Api.Features.Auth.GetMe;
using App.Api.Features.Auth.Login;
using App.Api.Features.Auth.Register;

namespace App.Api.Features.Auth;

public static class AuthModule
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .RequireAuthorization()
            .WithTags("Auth")
            .WithOpenApi();

        group.MapGetMeEndpoint();
        group.MapLoginEndpoint();
        group.MapRegisterEndpoint();
    }
}
