using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Security;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Auth.Commands.Register;

public sealed class RegisterHandler(
    IUserReader userReader,
    IUserWriter userWriter,
    IPasswordHasher passwordHasher)
    : ICommandHandler<RegisterCommand, Result<RegisterResult>>
{
    public async Task<Result<RegisterResult>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var email = command.Email.Trim().ToLowerInvariant();

        if (await userReader.ExistsByEmailAsync(email, ct))
            return Fail<RegisterResult>("conflict", "Email already registered.");

        var passwordHash = passwordHasher.Hash(command.Password);
        var user = new User(email, passwordHash, RoleIds.User); // IsActive defaults (false) in domain

        await userWriter.AddAsync(user, ct);
        await userWriter.SaveChangesAsync(ct);

        return Ok(new RegisterResult(user.Id));
    }
}