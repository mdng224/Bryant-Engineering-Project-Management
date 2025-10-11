using App.Application.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace App.Infrastructure.Auth;

public class JwtTokenService(IConfiguration config) : ITokenService
{
    public (string token, DateTimeOffset expiresAtUtc) CreateForUser(Guid userId, string email, string roleName)
    {
        var issuer              = config["Jwt:Issuer"];
        var audience            = config["Jwt:Audience"];
        var claims              = CreateClaims(userId, email, roleName);
        var expires             = DateTimeOffset.UtcNow.AddHours(1);
        var nowUtc              = DateTime.UtcNow;
        var signingCredentials  = CreateSigningCredentials();

        var jwtSecurityToken = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expires.UtcDateTime,
            signingCredentials: signingCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);

        return (token, expires);
    }

    private static Claim[] CreateClaims(Guid userId, string email, string roleName) =>
    [
        // Standard JWT claims
        new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
        new Claim(JwtRegisteredClaimNames.Email, email),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(
            JwtRegisteredClaimNames.Iat,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
            ClaimValueTypes.Integer64),

        // ASP.NET-friendly
        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
        new Claim(ClaimTypes.Name, email),

        // 🔑 Authorize(Roles="...") relies on this
        new Claim(ClaimTypes.Role, roleName),
    ];

    private SigningCredentials CreateSigningCredentials()
    {
        var keyB64 = config["Jwt:KeyBase64"];
        var keyRaw = config["Jwt:Key"];

        byte[] keyBytes = !string.IsNullOrWhiteSpace(keyB64)
            ? Convert.FromBase64String(keyB64)
            : Encoding.UTF8.GetBytes(keyRaw ?? throw new InvalidOperationException("Missing Jwt:Key or Jwt:KeyBase64"));

        if (keyBytes.Length < 32) // HS256 needs >= 256 bits
            throw new InvalidOperationException("JWT key must be at least 32 bytes.");

        var signingKey = new SymmetricSecurityKey(keyBytes);
        return new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
    }
}