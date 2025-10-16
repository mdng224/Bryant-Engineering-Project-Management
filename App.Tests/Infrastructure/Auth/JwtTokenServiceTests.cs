using App.Infrastructure.Auth;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace App.Tests.Infrastructure.Auth;

public class JwtTokenServiceTests
{
    private static IConfiguration CreateConfig(string? key = null, string? keyB64 = null)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "issuer",
                ["Jwt:Audience"] = "aud",
                ["Jwt:Key"] = key,
                ["Jwt:KeyBase64"] = keyB64
            })
            .Build();
    }

    [Fact]
    public void Throws_When_Key_Missing()
    {
        // Arrange
        var config = CreateConfig();
        var svc = new JwtTokenService(config);

        // Act
        var act = () => svc.CreateForUser(Guid.NewGuid(), "a@b.com", "User");

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("*Missing Jwt:Key*");
    }

    [Fact]
    public void Throws_When_Key_Too_Short()
    {
        // Arrange
        var config = CreateConfig(key: "short");
        var svc = new JwtTokenService(config);

        // Act
        var act = () => svc.CreateForUser(Guid.NewGuid(), "a@b.com", "User");

        // Assert
        act.Should()
           .Throw<InvalidOperationException>()
           .WithMessage("*at least 32 bytes*");
    }

    [Fact]
    public void Creates_Token_With_Claims_And_Expiry()
    {
        // Arrange
        var key = Convert.ToBase64String(Encoding.UTF8.GetBytes(new string('x', 32)));
        var config = CreateConfig(keyB64: key);
        var svc = new JwtTokenService(config);

        var userId = Guid.NewGuid();
        var (token, expires) = svc.CreateForUser(userId, "test@example.com", "Admin");

        // Act & Assert
        expires.Should().BeCloseTo(DateTimeOffset.UtcNow.AddHours(1), TimeSpan.FromSeconds(5));

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("issuer");
        jwt.Audiences.Should().Contain("aud");

        var claims = jwt.Claims.ToDictionary(c => c.Type, c => c.Value);
        claims.Should().ContainKey(JwtRegisteredClaimNames.Sub).WhoseValue.Should().Be(userId.ToString());
        claims[JwtRegisteredClaimNames.Email].Should().Be("test@example.com");
        claims[ClaimTypes.Role].Should().Be("Admin");
    }
}