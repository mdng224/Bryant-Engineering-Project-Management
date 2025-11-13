using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Users.Commands.RestoreUser;

public class RestoreUserHandler(IUserRepository repo, IUnitOfWork uow)
    : ICommandHandler<RestoreUserCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RestoreUserCommand command, CancellationToken ct)
    {
        var user = await repo.GetAsync(command.Id, ct);
        if (user is null)
            return Fail<Unit>(code: "not_found", message: "User not found.");

        if (!user.IsDeleted)  // Idempotent: already active
            return Ok(Unit.Value);

        if (!user.Restore())
            return Fail<Unit>(code: "restore_failed", message: "User could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<Unit>(
                code: "conflict",
                message: "Restoring this user conflicts with an existing active user.");
        }

        return Ok(Unit.Value);
    }
}