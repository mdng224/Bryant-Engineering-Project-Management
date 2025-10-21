using App.Application.Abstractions;
using App.Application.Admins.Commands.UpdateUser;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;
using System.Reflection;

namespace App.Tests.Application.Admins.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserWriter> _writer = new();
    private readonly Mock<IUserReader> _reader = new();
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _handler = new UpdateUserHandler(_reader.Object, _writer.Object);
    }

    [Fact]
    public async Task Returns_NoChangesSpecified_When_Nothing_Provided()
    {
        // Arrange
        var command = new UpdateUserCommand(Guid.NewGuid(), null, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.NoChangesSpecified);
        _writer.Verify(w => w.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_NotFound_Error_When_User_Does_Not_Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _writer.Setup(w => w.GetForUpdateAsync(userId, It.IsAny<CancellationToken>()))
               .ReturnsAsync((User?)null);

        var command = new UpdateUserCommand(userId, RoleNames.User, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error.Value.Message.Should().Be("User not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_NotFound_Error_When_RoleName_Invalid()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, "NotARole", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error.Value.Message.Should().Be("Role not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_Role_Only()
    {
        // Arrange
        var user = NewUser(RoleIds.Manager);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, RoleNames.User, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.User);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updates_Status_Only()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        MarkVerifiedForTests(user); // <-- ensure the domain precondition is met
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, null, true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.Status.Should().Be(UserStatus.Active);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task Updates_Both_Role_And_Status()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, RoleNames.Administrator, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.Administrator);
        user.Status.Should().Be(UserStatus.Disabled);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_RoleName_Before_Mapping()
    {
        // Arrange
        var user = NewUser(RoleIds.Manager);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, $"  {RoleNames.User}  ", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.User);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Returns_RoleNotFound_When_RoleName_Is_Whitespace_Even_If_Status_Provided()
    {
        // Arrange
        var user = NewUser(RoleIds.Manager);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        var command = new UpdateUserCommand(user.Id, "   ", true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");
        result.Error.Value.Message.Should().Be("Role not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Forbids_Demoting_Last_Active_Admin()
    {
        // Arrange: active admin, only one admin in system
        var admin = NewUser(RoleIds.Administrator);
        ActivateForTests(admin);

        _writer.Setup(w => w.GetForUpdateAsync(admin.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(admin);
        _reader.Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var command = new UpdateUserCommand(admin.Id, RoleNames.User, null); // demote

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("forbidden");
        admin.RoleId.Should().Be(RoleIds.Administrator); // unchanged
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Forbids_Deactivating_Last_Active_Admin()
    {
        // Arrange: active admin, only one admin in system
        var admin = NewUser(RoleIds.Administrator);
        ActivateForTests(admin);

        _writer.Setup(w => w.GetForUpdateAsync(admin.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(admin);
        _reader.Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(1);

        var command = new UpdateUserCommand(admin.Id, null, false); // deactivate

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("forbidden");
        admin.Status.Should().Be(UserStatus.Active); // unchanged
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Allows_Demoting_Admin_When_Multiple_Admins_Exist()
    {
        // Arrange: active admin, >1 admins in system
        var admin = NewUser(RoleIds.Administrator);
        ActivateForTests(admin);

        _writer.Setup(w => w.GetForUpdateAsync(admin.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(admin);
        _reader.Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(2);

        var command = new UpdateUserCommand(admin.Id, RoleNames.User, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        admin.RoleId.Should().Be(RoleIds.User);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Allows_Deactivating_Admin_When_Multiple_Admins_Exist()
    {
        // Arrange: active admin, >1 admins in system
        var admin = NewUser(RoleIds.Administrator);
        ActivateForTests(admin);

        _writer.Setup(w => w.GetForUpdateAsync(admin.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(admin);
        _reader.Setup(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()))
               .ReturnsAsync(2);

        var command = new UpdateUserCommand(admin.Id, null, false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        admin.Status.Should().Be(UserStatus.Disabled);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // -------- helpers --------
    private static User NewUser(Guid roleId) =>
        new(email: "user@example.com", passwordHash: "hash", roleId: roleId);

    /// <summary>
    /// Makes a user Active in tests. If the domain enforces "verified before activate",
    /// this will call MarkEmailVerified(now) if present before calling Activate().
    /// </summary>
    private static void ActivateForTests(User user)
    {
        // Try to call MarkEmailVerified(DateTimeOffset) if it exists
        var markVerified = typeof(User).GetMethod("MarkEmailVerified",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
            binder: null,
            types: [typeof(DateTimeOffset)],
            modifiers: null);

        markVerified?.Invoke(user, [DateTimeOffset.UtcNow]);

        user.Activate();
    }
    
    private static void MarkVerifiedForTests(User user)
    {
        // If you implemented MarkEmailVerified(DateTimeOffset), use it:
        var m = typeof(User).GetMethod(
            "MarkEmailVerified",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic,
            binder: null,
            types: [typeof(DateTimeOffset)],
            modifiers: null);

        if (m != null)
        {
            m.Invoke(user, [DateTimeOffset.UtcNow]);
        }
        else
        {
            // Fallback: if you exposed EmailVerifiedAt as a property setter or internal,
            // set it here via reflection (only as a test backdoor).
            var p = typeof(User).GetProperty("EmailVerifiedAt",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
            p?.SetValue(user, DateTimeOffset.UtcNow);
        }
    }

}
