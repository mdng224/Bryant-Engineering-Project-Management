using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Api.Contracts.Auth;
using App.Application.Auth.Commands.Register;
using App.Application.Auth.Queries.Login;

namespace App.Api.Features.Auth.Mappers;

internal static class AuthMappers
{
    public static RegisterCommand ToCommand(this RegisterRequest request) => new(request.Email, request.Password);

    public static LoginQuery ToQuery(this LoginRequest request) => new(request.Email, request.Password);

    public static LoginResponse ToResponse(this LoginResult result) =>
        new(result.Token, result.ExpiresAtUtc);

    public static MeResponse ToResponse(this ClaimsPrincipal user)
    {
        var subject = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing 'subject' claim.");

        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                    ?? user.FindFirst(ClaimTypes.Email)?.Value;

        return new MeResponse(userId.ToString(), email);
    }

    public static RegisterResponse ToResponse(this RegisterResult result)
        => new(result.UserId, "pending", "Your account is pending administrator approval.");
}
