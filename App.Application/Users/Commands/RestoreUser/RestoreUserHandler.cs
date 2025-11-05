using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Users.Commands.RestoreUser;

public class RestoreUserHandler(IUserReader reader, IUnitOfWork uow)
    : ICommandHandler<RestoreUserCommand, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(RestoreUserCommand cmd, CancellationToken ct)
    {
        var user = await reader.GetByIdAsync(cmd.Id, ct);
        if (user is null)
            return Fail<UserDto>(code: "not_found", message: "User not found.");

        if (!user.IsDeleted)  // Idempotent: already active
            return Ok(user.ToDto());

        if (!user.Restore())
            return Fail<UserDto>(code: "restore_failed", message: "User could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<UserDto>(
                code: "conflict",
                message: "Restoring this user conflicts with an existing active user.");
        }

        return Ok(user.ToDto());
    }
}