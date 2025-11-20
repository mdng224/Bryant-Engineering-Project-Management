using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth.GetMe;

public static class GetMeEndpoint
{
    public static RouteGroupBuilder MapGetMeEndpoint(this RouteGroupBuilder group)
    {
        // ---- GET /auth/me (requires Bearer token)
        group.MapGet("/me", Handle)
            .WithSummary("Get current user")
            .WithDescription("Returns subject and email extracted from the JWT.")
            .Produces<GetMeResponse>()
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }
    
    /// <summary>
    /// GET /auth/me – returns the authenticated user's ID and email
    /// from the JWT without a database lookup.
    /// </summary>
    /// <remarks>
    /// Requires a valid Bearer token.
    /// Returns 200 with <see cref="GetMeResponse"/> on success or 401 if unauthorized.
    /// </remarks>
    private static IResult Handle(ClaimsPrincipal user)
    {
        var response = user.ToResponse();
        
        return user.Identity?.IsAuthenticated is true ? Ok(response) : Unauthorized();
    }

    private static GetMeResponse ToResponse(this ClaimsPrincipal user)
    {
        var subject = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                      ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing 'subject' claim.");

        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value;

        return new GetMeResponse(userId.ToString(), email);
    }
}