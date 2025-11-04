using App.Api.Contracts.Auth;
using App.Api.Filters;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Queries.Login;
using System.Security.Claims;
using App.Api.Common.Mappers;
using App.Api.Features.Auth.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth")
            .RequireAuthorization()
            .WithTags("Auth")
            .WithOpenApi();
        
        // ---- GET /auth/me (requires Bearer token)
        group.MapGet("/me", HandleGetMe)
            .WithSummary("Get current user")
            .WithDescription("Returns subject and email extracted from the JWT.")
            .Produces<MeResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
        
        // ---- POST /auth/login
        group.MapPost("/login", HandleLogin)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<LoginRequest>>()
            .Accepts<LoginRequest>("application/json")
            .WithName("Auth_Login")
            .WithSummary("Login to the application")
            .WithDescription("Login with email and password to receive an access token.")
            .Produces<LoginResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        // ---- POST /auth/register
        group.MapPost("/register", HandleRegister)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<RegisterRequest>>()
            .Accepts<RegisterRequest>("application/json")
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// GET /auth/me – returns the authenticated user's ID and email
    /// from the JWT without a database lookup.
    /// </summary>
    /// <remarks>
    /// Requires a valid Bearer token.
    /// Returns 200 with <see cref="MeResponse"/> on success or 401 if unauthorized.
    /// </remarks>
    private static IResult HandleGetMe(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated is true
            ? Ok(user.ToResponse())
            : Unauthorized();
    }
    
    private static async Task<IResult> HandleLogin(
        [FromBody] LoginRequest request,
        [FromServices] IQueryHandler<LoginQuery, Result<LoginResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        return result.ToHttpResult(loginResult => Ok(loginResult.ToResponse()));
    }
    
    private static async Task<IResult> HandleRegister(
        [FromBody] RegisterRequest request,
        [FromServices] ICommandHandler<RegisterCommand, Result<RegisterResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await handler.Handle(command, ct);

        return result.ToHttpResult(registerResult =>
        {
            var registerResponse = registerResult.ToResponse();
            return Created($"/auth/users/{registerResponse.UserId}", registerResponse);
        });
    }
}
