using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionHandler(IPositionWriter writer)
    : ICommandHandler<UpdatePositionCommand, Result<PositionDto>>
{
    public async Task<Result<PositionDto>> Handle(UpdatePositionCommand command, CancellationToken ct)
    {
        var positionId = command.PositionId;
        var position = await writer.GetForUpdateAsync(positionId, ct);
        if (position is null)
            return Fail<PositionDto>(code: "not_found", message: "Position not found.");

        position.Update(command.Name, command.Code, command.RequiresLicense);
        
        try
        {
            await writer.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Fail<PositionDto>(
                code: "duplicate",
                message: "A position with the same unique field (e.g., code) already exists.");
        }

        return Ok(position.ToDto());
    }
}