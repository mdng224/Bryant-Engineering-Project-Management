using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Exceptions;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Repositories;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.AddPosition;

public class AddPositionHandler(IPositionReader reader, IPositionRepository repository, IUnitOfWork uow)
    : ICommandHandler<AddPositionCommand, Result<Guid>>
{
    private const string NameConflictCode = "conflict";
    private const string NameConflictMessage = "A position with this name exists.";
    public async Task<Result<Guid>> Handle(AddPositionCommand command, CancellationToken ct)
    {
        var normalizedName = command.Name.ToNormalizedName();
        var matchedPositions = await reader.GetByNameIncludingDeletedAsync(normalizedName, ct);
        var activePosition    = matchedPositions.FirstOrDefault(p => p.DeletedAtUtc is null);
        var tombstonePosition = matchedPositions.FirstOrDefault(p => p.DeletedAtUtc is not null);
        
        if (activePosition is not null)
            return Fail<Guid>(NameConflictCode, NameConflictMessage);
        
        if (tombstonePosition is not null)
        {
            tombstonePosition.RestoreAndUpdate(command.Name, command.Code, command.RequiresLicense);
            repository.Update(tombstonePosition);

            try
            {
                await uow.SaveChangesAsync(ct);
            }
            catch (UniqueConstraintViolationException)
            {
                return Fail<Guid>(NameConflictCode, NameConflictMessage);
            }

            return Ok(tombstonePosition.Id);
        }
        
        var position = command.ToDomain();

        try
        {
            repository.Add(position);
            await uow.SaveChangesAsync(ct);
        }
        catch (UniqueConstraintViolationException)
        {
            return Fail<Guid>(NameConflictCode, NameConflictMessage);
        }
        
        return Ok(position.Id);
    }
}