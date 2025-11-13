using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionHandler(IPositionRepository repo, IUnitOfWork uow)
    : ICommandHandler<UpdatePositionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(UpdatePositionCommand command, CancellationToken ct)
    {
        var positionId = command.PositionId;
        var position = await repo.GetForUpdateAsync(positionId, ct);
        if (position is null)
            return Fail<Unit>(code: "not_found", message: "Position not found.");

        position.Update(command.Name, command.Code, command.RequiresLicense);
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail<Unit>(
                code: "concurrency",
                message: "The position was modified by another process.");
        }
        catch (DbUpdateException)
        {
            return Fail<Unit>(
                code: "conflict",
                message: "A position with the same unique field (name/code) already exists.");
        }

        return Ok(Unit.Value);
    }
}