using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Positions.Queries.GetPositions;

public sealed class GetPositionsHandler(IPositionReader reader)
    : IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionListItemDto>>>
{
    public async Task<Result<PagedResult<PositionListItemDto>>> Handle(GetPositionsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedNameFilter = query.NameFilter?.ToNormalizedName();
        var (items, total) = await reader.GetPagedAsync(skip,
            pageSize,
            normalizedNameFilter,
            query.IsDeleted,
            ct);

        var pagedResult = new PagedResult<PositionListItemDto>(items, total, page, pageSize);
        
        return Ok(pagedResult);
    }
}