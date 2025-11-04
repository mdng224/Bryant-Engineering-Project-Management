using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Positions.Commands.DeletePosition;

public class DeletePositionHandler(IPositionWriter writer)
    : ICommandHandler<DeletePositionCommand, Result<Unit>>
{
    public async Task<Result<Unit>> Handle(DeletePositionCommand command, CancellationToken ct)
    {
        var deleted = await writer.SoftDeleteAsync(command.Id, ct);
        
        return deleted
            ? Ok(Unit.Value)
            : Fail<Unit>(code: "not_found", "User not found.");
    }
}