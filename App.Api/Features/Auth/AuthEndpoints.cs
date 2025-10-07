using App.Application.Abstractions;
using Microsoft.AspNetCore.Http.HttpResults;
using App.Api.Contracts.Auth;

namespace App.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .WithTags("Auth")
            .WithOpenApi();

        group.MapPost("/login", Login)
            .AllowAnonymous()
            .WithName("Auth_Login")
            .WithSummary("Login to the application")
            .WithDescription("Login with email and password to receive an access token.")
            .Produces<Ok<LoginResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/register", Register)
            .AllowAnonymous()
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<Created<string>>(StatusCodes.Status201Created)
            .Produces<ProblemHttpResult>(StatusCodes.Status400BadRequest);
    }

    private static async Task<Results<Ok<LoginResponse>, ProblemHttpResult, UnauthorizedHttpResult>>
        Login(LoginRequest dto, IAuthService auth, CancellationToken ct)
    {
        var result = await auth.LoginAsync(new(dto.Email, dto.Password), ct);
        if (!result.IsSuccess)
            return result.Error?.Code == "unauthorized"
                ? TypedResults.Unauthorized()
                : TypedResults.Problem(result.Error?.Message, statusCode: 400);

        return TypedResults.Ok(new LoginResponse(result.Value!.Token, result.Value.ExpiresAtUtc));
    }

    private static async Task<Results<Created<string>, ProblemHttpResult>>
        Register(RegisterRequest dto, IAuthService auth, CancellationToken ct)
    {
        var result = await auth.RegisterAsync(new(dto.Email, dto.Password), ct);
        if (!result.IsSuccess)
            return TypedResults.Problem(result.Error?.Message, statusCode: 400);

        return TypedResults.Created($"/auth/users/{result.Value!.UserId}", result.Value!.UserId.ToString());
    }
}
