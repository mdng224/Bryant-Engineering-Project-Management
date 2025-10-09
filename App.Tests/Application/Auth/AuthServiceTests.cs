using App.Application.Abstractions;
using App.Application.Auth;
using App.Domain.Users;
using Moq;
using FluentAssertions;

namespace App.Tests.Application.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_ReturnsInvalidRequest_WhenMissingEmailOrPassword()
    {
        // Arrange
        var authService = MakeService();
        LoginDto[] invalidInputs =
        [
            new LoginDto("", "pw"),
            new LoginDto("a@b.com", ""),
            new LoginDto(" ", " ")
        ];

        // Act
        var results = await Task.WhenAll(invalidInputs.Select(dto => authService.LoginAsync(dto, default)));

        // Assert
        foreach (var result in results)
        {
            result.IsSuccess.Should().BeFalse();
            result.Error?.Code.Should().Be("invalid_request");
        };
    }

    [Fact]
    public async Task LoginAsync_NormalizesEmail_AndSucceeds()
    {
        // Arrange
        var user = new User("user@example.com", "hash");

        var queries = new Mock<IUserQueries>();
        queries.Setup(q => q.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("pw", "hash")).Returns(true);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.CreateForUser(user.Id, user.Email))
              .Returns(("tok", DateTimeOffset.UtcNow.AddHours(1)));

        var authService = MakeService(queries: queries, hasher: hasher, tokens: tokens);

        // Act
        var res = await authService.LoginAsync(new LoginDto("  USER@example.com  ", "pw"), default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        tokens.Verify(t => t.CreateForUser(user.Id, user.Email), Times.Once);
        queries.Verify(q => q.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper to create AuthService with mock dependencies
    private static AuthService MakeService(
        Mock<IUserQueries>? queries = null,
        Mock<IUserCommands>? commands = null,
        Mock<IPasswordHasher>? hasher = null,
        Mock<ITokenService>? tokens = null)
    {
        queries ??= new Mock<IUserQueries>();
        commands ??= new Mock<IUserCommands>();
        hasher ??= new Mock<IPasswordHasher>();
        tokens ??= new Mock<ITokenService>();

        return new AuthService(queries.Object, commands.Object, hasher.Object, tokens.Object);
    }
}
