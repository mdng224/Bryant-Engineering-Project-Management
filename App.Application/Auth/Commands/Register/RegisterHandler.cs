using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Common;
using App.Domain.Security;
using App.Domain.Users;
using App.Domain.Users.Events;
using static App.Application.Common.R;

namespace App.Application.Auth.Commands.Register;

public sealed class RegisterHandler(
    IUserReader userReader,
    IUserWriter userWriter,
    IOutboxWriter outboxWriter,
    IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterCommand, Result<RegisterResult>>
{
    public async Task<Result<RegisterResult>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email.ToNormalizedEmail();

        if (await userReader.ExistsByEmailAsync(normalizedEmail, ct))
            return Fail<RegisterResult>("conflict", "Email already registered.");

        var passwordHash = passwordHasher.Hash(command.Password);
        var user = new User(normalizedEmail, passwordHash, RoleIds.User); // IsActive defaults (false) in domain
        
        await userWriter.AddAsync(user, ct);
        var userRegisteredEvent = new UserRegistered(user.Id, user.Email, user.Status);
        await outboxWriter.AddAsync(userRegisteredEvent, ct);
        await userWriter.SaveChangesAsync(ct);
        
        return Ok(new RegisterResult(user.Id));
    }
}