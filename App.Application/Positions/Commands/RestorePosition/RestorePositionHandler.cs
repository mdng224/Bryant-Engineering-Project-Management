using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.RestorePosition;

public class RestorePositionHandler(IPositionReader reader, IUnitOfWork uow)
    : ICommandHandler<RestorePositionCommand, Result<PositionListItemDto>>
{
    public async Task<Result<PositionListItemDto>> Handle(RestorePositionCommand cmd, CancellationToken ct)
    {
        var position = await reader.GetByIdAsync(cmd.Id, ct);
        if (position is null)
            return Fail<PositionListItemDto>(code: "not_found", message: "Position not found.");

        if (!position.IsDeleted)  // Idempotent: already active
            return Ok(position.ToDto());

        if (!position.Restore())
            return Fail<PositionListItemDto>(code: "restore_failed", message: "Position could not be restored.");
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // Another active row may now hold a unique Name/Code, etc.
            return Fail<PositionListItemDto>(
                code: "conflict",
                message: "Restoring this position conflicts with an existing active position.");
        }

        return Ok(position.ToDto());
    }
}