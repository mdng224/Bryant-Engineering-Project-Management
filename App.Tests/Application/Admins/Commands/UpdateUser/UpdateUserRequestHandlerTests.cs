using App.Application.Abstractions;
using App.Application.Admins.Commands.UpdateUser;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Admins.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserWriter> _writer = new();
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _handler = new UpdateUserHandler(_writer.Object);
    }

    [Fact]
    public async Task Returns_NoChangesSpecified_When_Nothing_Provided()
    {
        // Arrange
        var cmd = new UpdateUserCommand(Guid.NewGuid(), null, null);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(UpdateUserResult.NoChangesSpecified);
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

        var cmd = new UpdateUserCommand(userId, RoleNames.User, true);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error!.Value.Code.Should().Be("not_found");
        res.Error.Value.Message.Should().Be("User not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Returns_NotFound_Error_When_RoleName_Invalid()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var cmd = new UpdateUserCommand(user.Id, "NotARole", null);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error!.Value.Code.Should().Be("not_found");
        res.Error.Value.Message.Should().Be("Role not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Updates_Role_Only()
    {
        // Arrange
        var user = NewUser(RoleIds.Manager);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var cmd = new UpdateUserCommand(user.Id, RoleNames.User, null);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.User);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updates_Status_Only()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var cmd = new UpdateUserCommand(user.Id, null, true);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(UpdateUserResult.Ok);
        user.IsActive.Should().BeTrue();
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Updates_Both_Role_And_Status()
    {
        // Arrange
        var user = NewUser(RoleIds.User);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var cmd = new UpdateUserCommand(user.Id, RoleNames.Administrator, false);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.Administrator);
        user.IsActive.Should().BeFalse();
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Trims_RoleName_Before_Mapping()
    {
        // Arrange
        var user = NewUser(RoleIds.Manager);
        _writer.Setup(w => w.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var cmd = new UpdateUserCommand(user.Id, $"  {RoleNames.User}  ", null);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(UpdateUserResult.Ok);
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

        var cmd = new UpdateUserCommand(user.Id, "   ", true);

        // Act
        var res = await _handler.Handle(cmd, default);

        // Assert
        res.IsSuccess.Should().BeFalse();
        res.Error!.Value.Code.Should().Be("not_found");
        res.Error.Value.Message.Should().Be("Role not found.");
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }


    // -------- helpers --------
    private static User NewUser(Guid roleId) =>
        new(email: "user@example.com", passwordHash: "hash", roleId: roleId)
        {
            // if you need initial IsActive true/false, call domain methods here
        };
}