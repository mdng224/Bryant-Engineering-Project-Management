using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Positions;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Positions.Queries.GetPositions;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions.ListPositions;

public static class ListPositionsEndpoint
{
    public static RouteGroupBuilder MapListPositionsEndpoint(this RouteGroupBuilder group)
    {
        group.MapGet("", Handle)
            .WithSummary("List all positions (paginated)")
            .Produces<ListPositionsResponse>();

        return group;
    }
    
    private static async Task<IResult> Handle(
        [AsParameters] ListPositionsRequest request,
        [FromServices] IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionListItemDto>>> handler,
        CancellationToken ct)
    {
        var query  = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToResponse();
        return Ok(response);
    }

    private static GetPositionsQuery ToQuery(this ListPositionsRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        var getPositionsQuery = new GetPositionsQuery(pagedQuery, request.NameFilter, request.IsDeleted);
        
        return getPositionsQuery;
    }

    private static ListPositionsResponse ToResponse(this PagedResult<PositionListItemDto> pagedResult) =>
        new(
            pagedResult.Items.Select(pd => pd.ToResponse()).ToList(),
            pagedResult.TotalCount,
            pagedResult.Page,
            pagedResult.PageSize,
            pagedResult.TotalPages
        );

    private static PositionRowResponse ToResponse(this PositionListItemDto listItemDto) =>
        new(
            Id:              listItemDto.Id,
            Name:            listItemDto.Name,
            Code:            listItemDto.Code,
            RequiresLicense: listItemDto.RequiresLicense,
            DeletedAtUtc:    listItemDto.DeletedAtUtc
        );
}