using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Auth.Queries.Login;
using App.Application.Common.Results;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth.Login;

public static class LoginEndpoint
{
    public static RouteGroupBuilder MapLoginEndpoint(this RouteGroupBuilder group)
    {
        // ---- POST /auth/login
        group.MapPost("/login", Handle)
            .AllowAnonymous()
            .AddEndpointFilter<Validate<LoginRequest>>()
            .Accepts<LoginRequest>("application/json")
            .WithName("Auth_Login")
            .WithSummary("Login to the application")
            .WithDescription("Login with email and password to receive an access token.")
            .Produces<LoginResponse>()
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromBody] LoginRequest request,
        [FromServices] IQueryHandler<LoginQuery, Result<LoginResult>> handler,
        CancellationToken ct)
    {
        var query  = new LoginQuery(request.Email, request.Password);
        var result = await handler.Handle(query, ct);

        var loginResult = result.Value!;
        var response = new LoginResponse(loginResult.Token, loginResult.ExpiresAtUtc);

        return Ok(response);
    }
}