using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.RestorePosition;

public class RestorePositionHandler(IPositionRepository repo, IUnitOfWork uow)
    : ICommandHandler<RestorePositionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(RestorePositionCommand command, CancellationToken ct)
    {
        var position = await repo.GetAsync(command.Id, ct);
        if (position is null)
            return Fail<Unit>(code: "not_found", message: "Position not found.");

        if (!position.IsDeleted)  // Idempotent: already active
            return Ok(Unit.Value);

        if (!position.Restore())
            return Fail<Unit>(code: "restore_failed", message: "Position could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<Unit>(
                code: "conflict",
                message: "Restoring this position conflicts with an existing active position.");
        }

        return Ok(Unit.Value);
    }
}