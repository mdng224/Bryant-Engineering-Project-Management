using App.Application.Abstractions;
using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Security;
using App.Application.Auth.Commands.Register;
using App.Domain.Security;
using App.Domain.Users;
using App.Domain.Users.Events;
using FluentAssertions;
using Moq;

namespace App.Tests.Application.Auth.Commands.Register;

public sealed class RegisterHandlerTests
{
    private readonly Mock<IUserReader> _reader = new();
    private readonly Mock<IUserWriter> _writer = new();
    private readonly Mock<IOutboxWriter> _outbox = new();
    private readonly Mock<IPasswordHasher> _hasher = new();
    private readonly RegisterHandler _handler;

    public RegisterHandlerTests()
    {
        _handler = new RegisterHandler(_reader.Object, _writer.Object,  _outbox.Object, _hasher.Object);
    }

    [Fact]
    public async Task Returns_Conflict_When_Email_Already_Exists()
    {
        // Arrange
        const string rawEmail = "User@Example.com  ";
        _reader.Setup(r => r.ExistsByEmailAsync("user@example.com", It.IsAny<CancellationToken>()))
               .ReturnsAsync(true);

        var command = new RegisterCommand(rawEmail, "pw");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error!.Value.Code.Should().Be("conflict");
        result.Error.Value.Message.Should().Be("Email already registered.");

        _hasher.Verify(h => h.Hash(It.IsAny<string>()), Times.Never);
        _writer.Verify(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        _outbox.Verify(o => o.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Trims_And_Lowercases_Email_Hashes_Password_Persists_User_Writes_Outbox_And_Returns_Id()
    {
        // Arrange
        const string rawEmail = "  USER@Example.COM ";
        const string normalizedEmail = "user@example.com";
        const string password = "S3cret!";
        const string hash = "hashed_pw";

        _reader.Setup(r => r.ExistsByEmailAsync(normalizedEmail, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);
        _hasher.Setup(h => h.Hash(password)).Returns(hash);

        User? capturedUser = null;
        object? publishedEvent = null;

        _writer.Setup(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
               .Callback<User, CancellationToken>((u, _) => capturedUser = u)
               .Returns(Task.CompletedTask);

        _writer.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        _outbox.Setup(o => o.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()))
               .Callback<object, CancellationToken>((e, _) => publishedEvent = e)
               .Returns(Task.CompletedTask);

        var command = new RegisterCommand(rawEmail, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value!.UserId.Should().NotBe(Guid.Empty);

        _hasher.Verify(h => h.Hash(password), Times.Once);
        _writer.Verify(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
        _writer.Verify(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _outbox.Verify(o => o.AddAsync(It.IsAny<object>(), It.IsAny<CancellationToken>()), Times.Once);

        capturedUser.Should().NotBeNull();
        capturedUser!.Email.Should().Be(normalizedEmail);
        capturedUser.PasswordHash.Should().Be(hash);
        capturedUser.RoleId.Should().Be(RoleIds.User);

        // Assert the published outbox event
        publishedEvent.Should().BeOfType<UserRegistered>();
        var userRegistered = (UserRegistered)publishedEvent!;
        userRegistered.UserId.Should().Be(capturedUser.Id);
        userRegistered.Email.Should().Be(capturedUser.Email);
        userRegistered.Status.Should().Be(capturedUser.Status);
    }


    [Fact]
    public async Task Uses_Exact_Email_Normalization_As_Lookup_Key_And_Writes_Outbox()
    {
        // Arrange
        const string rawEmail = "\tMiXeD@Example.Com\n";
        const string normalizedEmail = "mixed@example.com";
        const string password = "pw";
        const string hash = "h";

        var sequence = new MockSequence();
        _reader.InSequence(sequence)
               .Setup(r => r.ExistsByEmailAsync(normalizedEmail, It.IsAny<CancellationToken>()))
               .ReturnsAsync(false);

        _hasher.Setup(h => h.Hash(password)).Returns(hash);
        _writer.Setup(w => w.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);
        _writer.Setup(w => w.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Returns(Task.CompletedTask);

        var command = new RegisterCommand(rawEmail, password);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _reader.Verify(r => r.ExistsByEmailAsync(normalizedEmail, It.IsAny<CancellationToken>()), Times.Once);
        _outbox.Verify(o => o.AddAsync(It.Is<object>(e => e is UserRegistered), It.IsAny<CancellationToken>()), Times.Once);
    }
}
