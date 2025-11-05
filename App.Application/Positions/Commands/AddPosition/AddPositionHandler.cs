using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.AddPosition;

public class AddPositionHandler(IPositionReader reader, IPositionWriter writer, IUnitOfWork uow)
    : ICommandHandler<AddPositionCommand, Result<PositionDto>>
{
    public async Task<Result<PositionDto>> Handle(AddPositionCommand command, CancellationToken ct)
    {
        var normalizedName = command.Name.ToNormalizedName();
        var matchedPositions = await reader.GetByNameIncludingDeletedAsync(normalizedName, ct);
        var activePosition    = matchedPositions.FirstOrDefault(p => p.DeletedAtUtc is null);
        var tombstonePosition = matchedPositions.FirstOrDefault(p => p.DeletedAtUtc is not null);
        
        if (activePosition is not null)
            return Fail<PositionDto>(code: "conflict", message: "A position with the same name exists.");
        
        if (tombstonePosition is not null)
        {
            tombstonePosition.RestoreAndUpdate(command.Name, command.Code, command.RequiresLicense);
            writer.Update(tombstonePosition);
            await uow.SaveChangesAsync(ct);
            
            return Ok(tombstonePosition.ToDto());
        }
        
        var position = command.ToDomain();
        writer.Add(position);
        await uow.SaveChangesAsync(ct);
        
        return Ok(position.ToDto());
    }
}