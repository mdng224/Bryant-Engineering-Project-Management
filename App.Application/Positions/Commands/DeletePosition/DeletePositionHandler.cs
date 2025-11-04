using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Writers;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.DeletePosition;

public class DeletePositionHandler(IPositionWriter writer, IUnitOfWork uow)
    : ICommandHandler<DeletePositionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeletePositionCommand command, CancellationToken ct)
    {
        var deleted = await writer.SoftDeleteAsync(command.Id, ct);
        
        if (!deleted)
            return Fail<Unit>(code: "not_found", "Position not found.");
        
        await uow.SaveChangesAsync(ct);
        return Ok(Unit.Value);
    }
}