using App.Api.Contracts.Auth;
using App.Api.Filters;
using App.Api.Mappers;
using App.Api.Mappers.Auth;
using App.Application.Abstractions;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Queries.Login;
using App.Application.Common;
using System.Security.Claims;
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

        // ---- POST /auth/login
        group.MapPost("/login", Login.Handle)
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
        group.MapPost("/register", Register.Handle)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<RegisterRequest>>()
            .Accepts<RegisterRequest>("application/json")
            .WithName("Auth_Register")
            .WithSummary("Register a new user")
            .WithDescription("Create a new user account with email and password.")
            .Produces<RegisterResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status409Conflict);

        // ---- GET /auth/me (requires Bearer token)
        group.MapGet("/me", GetMe.Handle)
            .WithSummary("Get current user")
            .WithDescription("Returns subject and email extracted from the JWT.")
            .Produces<MeResponse>()
            .Produces(StatusCodes.Status401Unauthorized);
    }
}
