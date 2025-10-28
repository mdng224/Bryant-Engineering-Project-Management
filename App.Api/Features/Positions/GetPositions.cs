using App.Api.Contracts.Positions;
using App.Api.Mappers.Positions;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Positions.Queries;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions;

public static class GetPositions
{
    public static async Task<IResult> Handle(
        [AsParameters] GetPositionsRequest request,
        IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToQuery();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);

        var response = result.Value!.ToResponse();

        return Ok(response);
    }
}