using App.Application.Abstractions;
using App.Application.Admins;
using App.Domain.Security;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Admins;

public class AdminServiceTests
{
    private readonly Mock<IUserQueries> _queries = new();
    private readonly Mock<IUserCommands> _commands = new();
    private readonly AdminService _svc;

    public AdminServiceTests() => _svc = new AdminService(_queries.Object, _commands.Object);

    [Fact]
    public async Task UpdateUserAsync_Returns_NoChangesSpecified_When_Nothing_Provided()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: null, isActive: null, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(AdminUpdateResult.NoChangesSpecified);
        _queries.Verify(q => q.ExistsByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _commands.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUserAsync_Returns_UserNotFound_When_User_Does_Not_Exist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: "User", isActive: true, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(AdminUpdateResult.UserNotFound);
        _commands.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUserAsync_Returns_RoleNotFound_When_RoleName_Invalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: "NotARole", isActive: null, default);

        // Assert
        res.IsSuccess.Should().BeTrue();
        res.Value.Should().Be(AdminUpdateResult.RoleNotFound);
        _commands.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task UpdateUserAsync_Updates_Role_Only()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: RoleNames.User, isActive: null, default);

        // Assert
        res.Value.Should().Be(AdminUpdateResult.Ok);
        _commands.Verify(c => c.UpdateAsync(
            userId,
            RoleIds.User,       // mapped GUID
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Updates_Status_Only()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: null, isActive: true, default);

        // Assert
        res.Value.Should().Be(AdminUpdateResult.Ok);
        _commands.Verify(c => c.UpdateAsync(
            userId,
            null,
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Updates_Both_Role_And_Status()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: RoleNames.Administrator, isActive: false, default);

        // Assert
        res.Value.Should().Be(AdminUpdateResult.Ok);
        _commands.Verify(c => c.UpdateAsync(
            userId,
            RoleIds.Administrator,
            false,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Trims_RoleName_Before_Mapping()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: $"  {RoleNames.User}  ", isActive: null, default);

        // Assert
        res.Value.Should().Be(AdminUpdateResult.Ok);
        _commands.Verify(c => c.UpdateAsync(
                userId,
                RoleIds.User,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateUserAsync_Ignores_Whitespace_RoleName_When_Status_Is_Provided()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _queries.Setup(q => q.ExistsByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // roleName is whitespace → treated as “no role change”
        // Act
        var res = await _svc.UpdateUserAsync(userId, roleName: "   ", isActive: true, default);

        // Assert
        res.Value.Should().Be(AdminUpdateResult.Ok);
        _commands.Verify(c => c.UpdateAsync(
            userId,
            null,       // no role change
            true,
            It.IsAny<CancellationToken>()), Times.Once);
    }
}