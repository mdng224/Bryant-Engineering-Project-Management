using App.Application.Abstractions;
using App.Application.Auth;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Auth;

public class AuthServiceTests
{
    [Fact]
    public async Task LoginAsync_NormalizesEmail_AndSucceeds()
    {
        // Arrange
        var user = new User("user@example.com", "hash", RoleIds.User);

        var role = new Role(RoleIds.User, RoleNames.User);
        typeof(User).GetProperty(nameof(User.Role))!
                    .SetValue(user, role);

        var normalized = "user@example.com";

        var queries = new Mock<IUserQueries>();
        queries.Setup(q => q.GetByEmailWithRoleAsync(normalized, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("pw", "hash")).Returns(true);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.CreateForUser(user.Id, user.Email, role.Name))
              .Returns(("tok", DateTimeOffset.UtcNow.AddHours(1)));

        var authService = MakeService(queries: queries, hasher: hasher, tokens: tokens);

        // Act
        var res = await authService.LoginAsync(new LoginDto("  USER@example.com  ", "pw"), default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        queries.Verify(q => q.GetByEmailWithRoleAsync(normalized, It.IsAny<CancellationToken>()), Times.Once);
        tokens.Verify(t => t.CreateForUser(user.Id, user.Email, role.Name), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordInvalid()
    {
        // Arrange
        var user = new User("user@example.com", "hash", RoleIds.User);

        var queries = new Mock<IUserQueries>();
        queries.Setup(q => q.GetByEmailWithRoleAsync("user@example.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Verify("bad", "hash")).Returns(false);

        var svc = MakeService(queries: queries, hasher: hasher);

        // Act
        var res = await svc.LoginAsync(new LoginDto("USER@example.com", "bad"), default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error!.Value.Code.Should().Be("unauthorized");
    }

    [Fact]
    public async Task RegisterAsync_NormalizesEmail_HashesPassword_AndCreates()
    {
        // Arrange
        var queries = new Mock<IUserQueries>();
        queries.Setup(q => q.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        var commands = new Mock<IUserCommands>();
        var created = new User("user@example.com", "hash", RoleIds.User);
        commands.Setup(c => c.CreateAsync("user@example.com", "HASHED", It.IsAny<CancellationToken>()))
                .ReturnsAsync(created);

        var hasher = new Mock<IPasswordHasher>();
        hasher.Setup(h => h.Hash("pw12345A")).Returns("HASHED");

        var svc = MakeService(queries: queries, commands: commands, hasher: hasher);

        // Act
        var res = await svc.RegisterAsync(new RegisterDto("  USER@example.com  ", "pw12345A"), default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        commands.Verify(c => c.CreateAsync("user@example.com", "HASHED", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsConflict_WhenEmailExists()
    {
        // Arrange
        var queries = new Mock<IUserQueries>();
        queries.Setup(q => q.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var svc = MakeService(queries: queries);

        // Act
        var res = await svc.RegisterAsync(new RegisterDto(" USER@example.com ", "pw12345A"), default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error!.Value.Code.Should().Be("conflict");
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
