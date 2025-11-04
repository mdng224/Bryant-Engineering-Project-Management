using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.EmployeePositions.Queries;

public sealed class GetPositionsForEmployeesHandler(IEmployeePositionReader reader)
    : IQueryHandler<GetPositionsForEmployeesQuery, Result<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>>>
{
    public async Task<Result<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>>> Handle(
        GetPositionsForEmployeesQuery query,
        CancellationToken ct)
    {
        if (query.EmployeeIds.Count == 0)
            return Ok<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>>(new Dictionary<Guid, IReadOnlyList<PositionMiniDto>>());
        
        var map = await reader.GetPositionsForEmployeesAsync(query.EmployeeIds, ct);
        
        return Ok(map);
    }
}