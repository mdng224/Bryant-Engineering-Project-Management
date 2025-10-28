using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using App.Domain.Employees;
using static App.Application.Common.R;

namespace App.Application.Positions.Queries.AddPosition;

public class AddPositionHandler(IPositionWriter writer)
    : ICommandHandler<AddPositionCommand, Result<AddPositionResult>>
{
    public async Task<Result<AddPositionResult>> Handle(AddPositionCommand command, CancellationToken ct)
    {
        var position = command.ToDomain();

        await writer.AddAsync(position, ct);
        await writer.SaveChangesAsync(ct);

        var result = position.ToResult();

        return Ok(result);
    }
}