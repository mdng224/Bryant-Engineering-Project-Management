using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using static App.Application.Common.R;

namespace App.Application.Positions.Queries;

public sealed class GetPositionsHandler(IPositionReader reader)
    : IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>>
{
    public async Task<Result<GetPositionsResult>> Handle(GetPositionsQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);
        var (positions, total) = await reader.GetPagedAsync(skip, pageSize, ct);

        var positionDtos = positions.ToDto().ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        
        var result = new GetPositionsResult(positionDtos, total, page, pageSize, totalPages);
        
        return Ok(result);
    }
}