using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Positions.Mappers;
using App.Domain.Employees;
using static App.Application.Common.R;

namespace App.Application.Positions.Queries.GetPositions;

public sealed class GetPositionsHandler(IPositionReader reader)
    : IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionDto>>>
{
    public async Task<Result<PagedResult<PositionDto>>> Handle(GetPositionsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);
        var (positions, total) = await reader.GetPagedAsync(skip, pageSize, ct);

        var pagedResult = new PagedResult<Position>(positions, total, page, pageSize)
            .Map(position => position.ToDto());
        
        return Ok(pagedResult);
    }
}