using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Users.Commands.UpdateUser;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Users.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
    private readonly Mock<IUserReader> _reader = new(MockBehavior.Strict);
    private readonly Mock<IUserRepository> _repo = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _handler = new UpdateUserHandler(_reader.Object, _repo.Object, _uow.Object);
    }

    [Fact]
    public async Task Returns_NoChangesSpecified_When_Nothing_Provided()
    {
        // Arrange
        var cmd = new UpdateUserCommand(Guid.NewGuid(), null, null);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.NoChangesSpecified);

        _repo.Verify(r => r.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _reader.Verify(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _repo.VerifyNoOtherCalls();
        _reader.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Returns_NotFound_When_User_Missing()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repo.Setup(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
             .ReturnsAsync((User?)null);

        var cmd = new UpdateUserCommand(id, RoleNames.User, UserStatus.Active);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");

        _repo.Verify(r => r.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _reader.Verify(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _repo.VerifyNoOtherCalls();
        _reader.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Updates_Role_Only()
    {
        // Arrange
        var user = new User("user@example.com", "hash", RoleIds.Manager);

        _repo.Setup(r => r.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
             .ReturnsAsync(user);

        _uow.Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var cmd = new UpdateUserCommand(user.Id, RoleNames.User, null);

        // Act
        var result = await _handler.Handle(cmd, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.User);

        _repo.Verify(r => r.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        // Not an active admin, so we should never inspect admin count
        _reader.Verify(r => r.CountActiveAdminsAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _repo.VerifyNoOtherCalls();
        _reader.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }
}
