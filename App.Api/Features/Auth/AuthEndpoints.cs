using App.Api.Contracts.Auth;
using App.Api.Filters;
using App.Api.Mappers;
using App.Application.Abstractions;
using System.Security.Claims;

namespace App.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .RequireAuthorization()
            .WithTags("Auth")
            .WithOpenApi();

        // ---- POST /auth/login
        group.MapPost("/login", Login)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<LoginRequest>>()
            .Accepts<LoginRequest>("application/json")
            .WithName("Auth_Login")
            .WithSummary("Login to the application")
            .WithDescription("Login with email and password to receive an access token.")
            .Produces<LoginResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        // ---- POST /auth/register
        group.MapPost("/register", Register)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<RegisterRequest>>()
            .Accepts<RegisterRequest>("application/json")
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);

        // ---- GET /auth/me (requires Bearer token)
        group.MapGet("/me", GetMe)
            .WithSummary("Get current user")
            .WithDescription("Returns subject and email extracted from the JWT.")
            .Produces<MeResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<IResult> Login(
        LoginRequest loginRequest,
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.LoginAsync(loginRequest.ToDto(), ct);

        return result.ToHttpResult(loginResult => Results.Ok(loginResult.ToResponse()));
    }

    private static async Task<IResult> Register(
        RegisterRequest registerRequest,
        IAuthService authService,
        CancellationToken ct)
    {
        var result = await authService.RegisterAsync(registerRequest.ToDto(), ct);

        return result.ToHttpResult(registerResult =>
        {
            var registerResponse = registerResult.ToResponse();
            return Results.Created($"/auth/users/{registerResponse.UserId}", registerResponse);
        });
    }

    /// <summary>
    /// Retrieves minimal identity information for the currently authenticated user,
    /// extracted directly from the JSON Web Token (JWT) without a database lookup.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Route:</b> <c>GET /auth/me</c><br/>
    /// <b>Authorization:</b> Requires a valid <c>Bearer &lt;token&gt;</c> header.
    /// </para>
    /// <para>
    /// This endpoint is typically used by the frontend to verify an existing session
    /// and rehydrate client authentication state after login or token refresh.
    /// </para>
    /// <para>
    /// <b>Responses:</b><br/>
    /// <b>200 OK</b> – Returns the user’s subject (unique identifier) and email extracted from the JWT.<br/>
    /// <b>401 Unauthorized</b> – Returned when the token is missing, invalid, or expired.
    /// </para>
    /// </remarks>
    /// <returns>
    /// An <see cref="IResult"/> containing either a <see cref="MeResponse"/> on success
    /// or an HTTP 401 Unauthorized result when authentication fails.
    /// </returns>
    private static IResult GetMe(ClaimsPrincipal user)
    {
        try
        {
            return Results.Ok(user.ToResponse());
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
    }
}
