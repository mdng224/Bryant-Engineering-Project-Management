using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using Microsoft.EntityFrameworkCore;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.AddPosition;

public class AddPositionHandler(IPositionWriter writer)
    : ICommandHandler<AddPositionCommand, Result<PositionDto>>
{
    public async Task<Result<PositionDto>> Handle(AddPositionCommand command, CancellationToken ct)
    {
        var position = command.ToDomain();
        await writer.AddAsync(position, ct);
        
        try
        {
            await writer.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            return Fail<PositionDto>(
                code: "conflict",
                message: "A position with the same name or code already exists.");
        }

        var positionDto = position.ToDto();
        
        return Ok(positionDto);
    }
}