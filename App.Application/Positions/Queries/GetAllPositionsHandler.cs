using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using static App.Application.Common.R;

namespace App.Application.Positions.Queries;

public sealed class GetAllPositionsHandler(IPositionReader reader)
    : IQueryHandler<GetAllPositionsQuery, Result<GetAllPositionsResult>>
{
    public async Task<Result<GetAllPositionsResult>> Handle(GetAllPositionsQuery query, CancellationToken ct)
    {
        var positions = await reader.GetAllAsync(ct);
        if (positions.Count == 0)
            return Fail<GetAllPositionsResult>("no_positions", "No positions found.");

        var result = positions.ToResult();
        
        return Ok(result);
    }
}