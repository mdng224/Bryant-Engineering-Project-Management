using App.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace App.Infrastructure.Auth;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTimeOffset expiresAtUtc) CreateForUser(Guid userId, string email)
    {
        var issuer = _config["Jwt:Issuer"];
        var audience = _config["Jwt:Audience"];
        var claims = CreateClaims(userId, email);
        var expires = DateTimeOffset.UtcNow.AddHours(1);
        var signingCredentials = CreateSigningCredentials();

        var token = new JwtSecurityToken(issuer, audience, claims, null, expires.UtcDateTime, signingCredentials);
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return (jwt, expires);
    }

    private static Claim[] CreateClaims(Guid userId, string email) =>
    [
        new(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new(JwtRegisteredClaimNames.Email, email),
        new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
    ];

    private SigningCredentials CreateSigningCredentials()
    {
        var keyB64 = _config["Jwt:KeyBase64"];
        var keyRaw = _config["Jwt:Key"];

        byte[] keyBytes = !string.IsNullOrWhiteSpace(keyB64)
            ? Convert.FromBase64String(keyB64)
            : Encoding.UTF8.GetBytes(keyRaw ?? throw new InvalidOperationException("Missing Jwt:Key or Jwt:KeyBase64"));

        if (keyBytes.Length < 32) // HS256 needs >= 256 bits
            throw new InvalidOperationException("JWT key must be at least 32 bytes.");

        var signingKey = new SymmetricSecurityKey(keyBytes);
        return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }
}