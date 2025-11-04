using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Application.Users.Commands.UpdateUser;
using App.Domain.Security;
using App.Domain.Users;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Users.Commands.UpdateUser;

public class UpdateUserHandlerTests
{
     private readonly Mock<IUserWriter> _writer = new(MockBehavior.Strict);
    private readonly Mock<IUserReader> _reader = new(MockBehavior.Strict);
    private readonly Mock<IUnitOfWork> _uow = new(MockBehavior.Strict);
    private readonly UpdateUserHandler _handler;

    public UpdateUserHandlerTests()
    {
        _handler = new UpdateUserHandler(_reader.Object, _writer.Object, _uow.Object);
    }

    [Fact]
    public async Task Returns_NoChangesSpecified_When_Nothing_Provided()
    {
        var cmd = new UpdateUserCommand(Guid.NewGuid(), null, null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.NoChangesSpecified);

        _writer.Verify(uw => uw.GetForUpdateAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // or CommitAsync
        _reader.VerifyNoOtherCalls();
        _writer.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Returns_NotFound_When_User_Missing()
    {
        var id = Guid.NewGuid();
        _writer.Setup(w => w.GetForUpdateAsync(id, It.IsAny<CancellationToken>()))
               .ReturnsAsync((User?)null);

        var cmd = new UpdateUserCommand(id, RoleNames.User, UserStatus.Active);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("not_found");

        _writer.Verify(uw => uw.GetForUpdateAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never); // or CommitAsync
        _reader.VerifyNoOtherCalls();
        _writer.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Updates_Role_Only()
    {
        var user = new User("user@example.com", "hash", RoleIds.Manager);
        _writer.Setup(uw => uw.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()))
               .ReturnsAsync(user);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var cmd = new UpdateUserCommand(user.Id, RoleNames.User, null);

        var result = await _handler.Handle(cmd, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(UpdateUserResult.Ok);
        user.RoleId.Should().Be(RoleIds.User);

        _writer.Verify(uw => uw.GetForUpdateAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        _uow.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // or CommitAsync
        _reader.VerifyNoOtherCalls();
        _writer.VerifyNoOtherCalls();
        _uow.VerifyNoOtherCalls();
    }
}
