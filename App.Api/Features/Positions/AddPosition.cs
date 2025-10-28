using App.Api.Contracts.Positions;
using App.Api.Mappers.Positions;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Positions.Queries.AddPosition;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions;

public static class AddPosition
{
    public static async Task<IResult> Handle(
        [AsParameters] AddPositionRequest request,
        IQueryHandler<AddPositionCommand, Result<AddPositionResult>> handler,
        CancellationToken ct)
    {
        var query = request.ToCommand();
        var result = await handler.Handle(query, ct);

        if (!result.IsSuccess)
            return BadRequest(error: new { message = result.Error!.Value.Message });
        
        var response = result.Value!.ToResponse();

        return Created($"/positions/{response.Id}", response);
    }
}