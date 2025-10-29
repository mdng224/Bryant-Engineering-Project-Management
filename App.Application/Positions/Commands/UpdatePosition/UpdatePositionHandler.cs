using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Mappers;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.UpdatePosition;

public sealed class UpdatePositionHandler(IPositionWriter writer)
    : ICommandHandler<UpdatePositionCommand, Result<PositionResult>>
{
    public async Task<Result<PositionResult>> Handle(UpdatePositionCommand command, CancellationToken ct)
    {
        var positionId = command.PositionId;
        
        var position = await writer.GetForUpdateAsync(positionId, ct);
        if (position is null)
            return Fail<PositionResult>("not_found", "Position not found.");

        position.Rename(command.Name);
        position.SetCode(command.Code);
        position.RequireLicense(command.RequiresLicense);
        await writer.SaveChangesAsync(ct);

        var result = position.ToResult();
        
        return Ok(result);
    }
}