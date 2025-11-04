using App.Application.Abstractions;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Security;
using App.Application.Auth.Queries.Login;
using App.Domain.Security;   // <-- for RoleIds / ToName()
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Auth.Queries;

public sealed class LoginHandlerTests
{
    private readonly Mock<IUserReader> _users = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly Mock<ITokenService> _tokens = new();
    private readonly LoginHandler _handler;

    public LoginHandlerTests()
    {
        _handler = new LoginHandler(_users.Object, _hasher.Object, _tokens.Object);
    }

    [Fact]
    public async Task Succeeds_For_Active_User_With_Correct_Password()
    {
        // Arrange
        const string inputEmail = "  USER@Example.COM  ";
        const string normalized = "user@example.com";

        var user = NewUser(email: normalized, passwordHash: "hash", roleId: RoleIds.Administrator);
        SetStatus(user, UserStatus.Active);

        _users.Setup(r => r.GetByEmailAsync(normalized, It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        _hasher.Setup(h => h.Verify("pw", "hash")).Returns(true);

        var exp = DateTimeOffset.UtcNow.AddHours(1);
        _tokens.Setup(t => t.CreateForUser(user.Id, user.Email, RoleIds.Administrator.ToName()))
               .Returns(("jwt-token", exp));

        var query = new LoginQuery(inputEmail, "pw");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Token.Should().Be("jwt-token");
        result.Value.ExpiresAtUtc.Should().BeCloseTo(exp, TimeSpan.FromSeconds(1));
        _tokens.Verify(t => t.CreateForUser(user.Id, normalized, RoleIds.Administrator.ToName()), Times.Once);
    }

    [Fact]
    public async Task Unauthorized_When_User_Not_Found()
    {
        // Arrange
        _users.Setup(r => r.GetByEmailAsync("ghost@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync((User?)null);

        var query = new LoginQuery("ghost@example.com", "pw");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("unauthorized");
        _tokens.Verify(t => t.CreateForUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Unauthorized_When_Password_Is_Wrong()
    {
        // Arrange
        var user = NewUser("user@example.com", "hash", RoleIds.User);
        SetStatus(user, UserStatus.Active);

        _users.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        _hasher.Setup(h => h.Verify("bad", "hash")).Returns(false);

        var query = new LoginQuery("user@example.com", "bad");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("unauthorized");
        _tokens.Verify(t => t.CreateForUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(UserStatus.PendingEmail,    "forbidden", "Please verify your email before logging in.")]
    [InlineData(UserStatus.PendingApproval, "forbidden", "Your account is pending administrator approval.")]
    [InlineData(UserStatus.Disabled,        "forbidden", "Your account has been disabled.")]
    [InlineData(UserStatus.Denied,          "forbidden", "Your registration was denied by an administrator.")]
    public async Task Forbidden_For_NonActive_Status(UserStatus status, string code, string message)
    {
        // Arrange
        var user = NewUser("user@example.com", "hash", RoleIds.User);
        SetStatus(user, status);

        _users.Setup(r => r.GetByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
              .ReturnsAsync(user);

        // Password verification won't be reached, but keep it true to avoid noise
        _hasher.Setup(h => h.Verify(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var query = new LoginQuery("user@example.com", "pw");

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be(code);
        result.Error.Value.Message.Should().Be(message);
        _tokens.Verify(t => t.CreateForUser(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    // -------- helpers --------
    private static User NewUser(string email, string passwordHash, Guid roleId)
        => new(email, passwordHash, roleId);

    private static void SetStatus(User user, UserStatus status)
    {
        // If Status has a private setter, use reflection to set it for tests.
        var prop = typeof(User).GetProperty(nameof(User.Status),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        prop!.SetValue(user, status);
    }
}
