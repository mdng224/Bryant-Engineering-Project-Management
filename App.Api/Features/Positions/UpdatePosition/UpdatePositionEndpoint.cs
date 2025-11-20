using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Positions.Commands.UpdatePosition;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions.UpdatePosition;

public static class UpdatePositionEndpoint
{
    public static RouteGroupBuilder MapUpdatePositionEndpoint(this RouteGroupBuilder group)
    {
        // PATCH /positions/{id}
        group.MapPatch("/{id:guid}", Handle)
            .AddEndpointFilter<Validate<UpdatePositionRequest>>() // TODO: Remove
            .WithSummary("Update a position")
            .Accepts<UpdatePositionRequest>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromBody] UpdatePositionRequest request,
        [FromServices] ICommandHandler<UpdatePositionCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand(id);
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();
        
        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found"  => NotFound(new { message = error.Message }),
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            "conflict"   => Conflict(new { message = error.Message }),
            "validation" => ValidationProblem(new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            _            => Problem(error.Message)
        };
    }

    private static UpdatePositionCommand ToCommand(this UpdatePositionRequest request, Guid positionId) =>
        new(
            PositionId:      positionId,
            Name:            request.Name,
            Code:            request.Code,
            RequiresLicense: request.RequiresLicense
        );
}