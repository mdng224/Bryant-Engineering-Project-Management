using App.Api.Contracts.Auth;
using App.Application.Auth;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace App.Api.Mappers;

public static class AuthMappers
{
    public static LoginDto ToDto(this LoginRequest loginRequest) =>
        new(loginRequest.Email, loginRequest.Password);

    public static RegisterDto ToDto(this RegisterRequest registerRequest) =>
        new(registerRequest.Email, registerRequest.Password);

    public static LoginResponse ToResponse(this LoginResult loginResult) =>
        new(loginResult.Token, loginResult.ExpiresAtUtc);

    public static MeResponse ToResponse(this ClaimsPrincipal user)
    {
        var subject = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(subject) || !Guid.TryParse(subject, out var userId))
            throw new UnauthorizedAccessException("Invalid or missing 'subject' claim.");

        var email = user.FindFirst(JwtRegisteredClaimNames.Email)?.Value
                 ?? user.FindFirst(ClaimTypes.Email)?.Value;

        return new(userId.ToString(), email);
    }

    public static RegisterResponse ToResponse(this RegisterResult result)
        => new(result.UserId);
}
