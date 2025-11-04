using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Application.Abstractions.Security;
using App.Application.Common;
using App.Application.Common.Results;
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
    IPasswordHasher passwordHasher,
    IUnitOfWork uow)
    : ICommandHandler<RegisterCommand, Result<RegisterResult>>
{
    private const string ConflictCode = "conflict";
    private const string EmailInUse = "Email already registered.";

    public async Task<Result<RegisterResult>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var normalizedEmail = command.Email.ToNormalizedEmail();

        if (await userReader.ExistsByEmailAsync(normalizedEmail, ct))
            return Fail<RegisterResult>(ConflictCode, EmailInUse);

        var passwordHash = passwordHasher.Hash(command.Password);
        var user = new User(normalizedEmail, passwordHash, RoleIds.User); // IsActive defaults (false) in domain
        
        userWriter.Add(user);
        
        var userRegisteredEvent = new UserRegistered(user.Id, user.Email, user.Status);
        outboxWriter.Add(userRegisteredEvent);
        
        try
        {
            await uow.SaveChangesAsync(ct); // single atomic commit
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            // covers email uniqueness race with another request
            return Fail<RegisterResult>(ConflictCode, EmailInUse);
        }
        
        return Ok(new RegisterResult(user.Id));
    }
}