using System.Security.Claims;
using App.Api.Contracts.Auth;
using App.Api.Mappers.Auth;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Auth;

public static class GetMe
{
    /// <summary>
    /// GET /auth/me – returns the authenticated user's ID and email
    /// from the JWT without a database lookup.
    /// </summary>
    /// <remarks>
    /// Requires a valid Bearer token.
    /// Returns 200 with <see cref="MeResponse"/> on success or 401 if unauthorized.
    /// </remarks>
    public static IResult Handle(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated is true
            ? Ok(user.ToResponse())
            : Unauthorized();
    }
        
}