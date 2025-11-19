using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
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
    private readonly Mock<IEmailVerificationRepository> _verRepo = new();
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IEmployeeReader> _employeeReader = new();
    private readonly Mock<IUnitOfWork> _uow = new();

    private VerifyEmailHandler CreateSut() =>
        new(_verRepo.Object, _employeeReader.Object, _userRepo.Object, _uow.Object);

    private static EmailVerification Verification(Guid userId, DateTime expiresAtUtc) =>
        new(userId, tokenHash: "hash", expiresAtUtc);

    private static EmailVerification UsedVerification(Guid userId, DateTime expiresAtUtc)
    {
        var ev = Verification(userId, expiresAtUtc);
        ev.MarkUsed();
        return ev;
    }

    private static User PendingUser(string email) =>
        new(email, passwordHash: "hash", roleId: RoleIds.User);

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

        _verRepo.VerifyNoOtherCalls();
        _userRepo.VerifyNoOtherCalls();
        _employeeReader.VerifyNoOtherCalls();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Token_not_found_returns_invalid_and_no_writes()
    {
        // Arrange
        var sut = CreateSut();
        _verRepo.Setup(r => r.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                .ReturnsAsync((EmailVerification?)null);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Invalid);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Already_used_token_returns_already_used_and_no_writes()
    {
        // Arrange
        var sut = CreateSut();
        var ev = UsedVerification(Guid.NewGuid(), DateTime.UtcNow.AddMinutes(10));

        _verRepo.Setup(evr => evr.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ev);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.AlreadyUsed);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Expired_token_returns_expired_and_no_writes()
    {
        // Arrange
        var sut = CreateSut();
        var ev = Verification(Guid.NewGuid(), DateTime.UtcNow.AddSeconds(-1));

        _verRepo.Setup(evr => evr.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ev);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Expired);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PendingEmail_with_employee_marks_verified_and_activated_and_saves()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var ev = Verification(userId, DateTime.UtcNow.AddMinutes(5));
        var user = PendingUser("alice@company.com");

        _verRepo.Setup(r => r.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
            .ReturnsAsync(ev);

        _userRepo.Setup(ur => ur.GetForUpdateAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var employee = new Employee(
            firstName:        "Alice",
            lastName:         "Example",
            preferredName:    null,
            userId:           null,               // not linked yet
            employmentType:   null,
            salaryType:       null,
            department:       null,
            hireDate:         null,
            companyEmail:     "alice@company.com",
            workLocation:     null,
            notes:            null,
            addressLine1:     null,
            addressLine2:     null,
            city:             null,
            state:            null,
            postalCode:       null,
            recommendedRoleId: null,
            isPreapproved:    false               // avoids preapproval/email invariant issues
        );

        _employeeReader.Setup(er => er.GetByCompanyEmailAsync("alice@company.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        user.Status.Should().Be(UserStatus.Active);
        ev.Used.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task PendingEmail_without_employee_marks_pending_approval_and_saves()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var ev = Verification(userId, DateTime.UtcNow.AddMinutes(5));
        var user = PendingUser("bob@other.com");

        _verRepo.Setup(r => r.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ev);

        _userRepo.Setup(ur => ur.GetForUpdateAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

        _employeeReader.Setup(er => er.GetByCompanyEmailAsync("bob@other.com", It.IsAny<CancellationToken>()))
                       .ReturnsAsync((Employee?)null);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        user.Status.Should().Be(UserStatus.PendingApproval);
        ev.Used.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Non_pending_user_still_marks_verification_used_and_saves()
    {
        // Arrange
        var sut = CreateSut();
        var userId = Guid.NewGuid();
        var ev = Verification(userId, DateTime.UtcNow.AddMinutes(5));

        var user = new User("charlie@company.com", "hash", RoleIds.User);
        user.MarkEmailVerified();
        user.Activate(); // ensure not PendingEmail

        _verRepo.Setup(evr => evr.GetForUpdateByTokenHashAsync("t", It.IsAny<CancellationToken>()))
                .ReturnsAsync(ev);

        _userRepo.Setup(ur => ur.GetForUpdateAsync(userId, It.IsAny<CancellationToken>()))
                 .ReturnsAsync(user);

        // Act
        var result = await sut.Handle(new VerifyEmailCommand("t"), CancellationToken.None);

        // Assert
        result.Value!.Outcome.Should().Be(VerifyEmailOutcome.Ok);
        user.Status.Should().NotBe(UserStatus.PendingEmail);
        ev.Used.Should().BeTrue();
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
