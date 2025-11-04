using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionHandler(IPositionReader reader, IUnitOfWork uow)
    : ICommandHandler<UpdatePositionCommand, Result<PositionDto>>
{
    public async Task<Result<PositionDto>> Handle(UpdatePositionCommand command, CancellationToken ct)
    {
        var positionId = command.PositionId;
        var position = await reader.GetForUpdateAsync(positionId, ct);
        if (position is null)
            return Fail<PositionDto>(code: "not_found", message: "Position not found.");

        position.Update(command.Name, command.Code, command.RequiresLicense);
        
        try
        {
            await uow.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Fail<PositionDto>(
                code: "concurrency",
                message: "The position was modified by another process.");
        }
        catch (DbUpdateException)
        {
            return Fail<PositionDto>(
                code: "conflict",
                message: "A position with the same unique field (name/code) already exists.");
        }

        return Ok(position.ToDto());
    }
}