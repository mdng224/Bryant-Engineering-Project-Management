using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using App.Domain.Employees;
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
            return Fail<PositionDto>("not_found", "Position not found.");

        position.Rename(command.Name);
        position.SetCode(command.Code);
        position.RequireLicense(command.RequiresLicense);
        await writer.SaveChangesAsync(ct);

        var positionDto = position.ToDto();
        
        return Ok(positionDto);
    }
}