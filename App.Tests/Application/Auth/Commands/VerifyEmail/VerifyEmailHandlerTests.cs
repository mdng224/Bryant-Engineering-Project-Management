using App.Application.Abstractions;
using App.Application.Auth.Commands.VerifyEmail;
using App.Domain.Auth;
using App.Domain.Employees;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandlerTests
{
    private readonly Mock<IEmailVerificationReader> _verReader = new();
    private readonly Mock<IEmailVerificationWriter> _verWriter = new();
    private readonly Mock<IUserReader> _userReader = new();
    private readonly Mock<IUserWriter> _userWriter = new();
    private readonly Mock<IEmployeeReader> _employeeReader = new();
    
    private VerifyEmailHandler CreateSut() =>
        new(_verReader.Object, _verWriter.Object, _userReader.Object, _userWriter.Object, _employeeReader.Object);
    
    private static EmailVerification Verification(Guid userId, DateTime expiresAtUtc)
        => new EmailVerification(userId, tokenHash: "hash", expiresAtUtc);

    private static EmailVerification UsedVerification(Guid userId, DateTime expiresAtUtc)
    {
        var v = Verification(userId, expiresAtUtc);
        v.MarkUsed();
        return v;
    }

    private static User PendingUser(string email)
    {
        var u = new User(email, passwordHash: "hash", roleId: RoleIds.User);
        // Do NOT call MarkPendingApproval() here — that’s the post-verify state.
        return u;
    }
    
    // =============== TESTS (AAA) ===============

    [Fact]
    public async Task Empty_token_returns_invalid_and_no_side_effects()
    {
        // Arrange
        var sut = CreateSut();
        var cmd = new VerifyEmailCommand("");

        // Act
        var result = await sut.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Invalid);

        _verReader.VerifyNoOtherCalls();
        _verWriter.VerifyNoOtherCalls();
        _userReader.VerifyNoOtherCalls();
        _userWriter.VerifyNoOtherCalls();
        _employeeReader.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Token_not_found_returns_invalid_and_no_writes()
    {
        // Arrange
        var sut = CreateSut();
        _verReader.Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                  .ReturnsAsync((EmailVerification?)null);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Invalid);
        _userWriter.VerifyNoOtherCalls();
        _verWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Already_used_token_returns_already_used()
    {
        // Arrange
        var sut = CreateSut();
        var v = UsedVerification(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(10));

        _verReader.Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
            .ReturnsAsync(v);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.AlreadyUsed);
        _userWriter.VerifyNoOtherCalls();
        _verWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Expired_token_returns_expired()
    {
        // Arrange
        var sut = CreateSut();
        var v = Verification(Guid.NewGuid(), DateTime.UtcNow.AddSeconds(-1));

        _verReader.Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
            .ReturnsAsync(v);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Expired);
        _userWriter.VerifyNoOtherCalls();
        _verWriter.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task PendingEmail_with_employee_activates_and_updates_both()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var v = Verification(userId, DateTime.UtcNow.AddMinutes(5));
        var user = PendingUser("alice@company.com");

        _verReader
            .Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
            .ReturnsAsync(v);

        _userReader
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _employeeReader
            .Setup(r => r.GetByCompanyEmailAsync("alice@company.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Employee("Alice", "Example"));

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        _userWriter.Verify(w => w.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _verWriter.Verify(w => w.UpdateAsync(v, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PendingEmail_without_employee_marks_pending_approval_and_updates_both()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var v = Verification(userId, DateTime.UtcNow.AddMinutes(5));
        var user = PendingUser("bob@other.com");

        _verReader.Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(v);
        _userReader.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(user);
        _employeeReader.Setup(r => r.GetByCompanyEmailAsync("bob@other.com", It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Employee?)null);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        _userWriter.Verify(w => w.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _verWriter.Verify(w => w.UpdateAsync(v, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Non_pending_user_still_marks_verification_used_and_updates_both()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var v = Verification(userId, DateTime.UtcNow.AddMinutes(5));
        var user = new User("charlie@company.com", "hash", RoleIds.User); // already active or not PendingEmail

        _verReader.Setup(r => r.GetByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(v);
        _userReader.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(user);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        _userWriter.Verify(w => w.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        _verWriter.Verify(w => w.UpdateAsync(v, It.IsAny<CancellationToken>()), Times.Once);
    }
}