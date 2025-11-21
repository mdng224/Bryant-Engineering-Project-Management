using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Positions.Commands.AddPosition;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions.AddPosition;

public static class AddPositionEndpoint
{
    public static RouteGroupBuilder MapAddPositionEndpoint(this RouteGroupBuilder group)
    {
        group.MapPost("", Handle)
            .WithSummary("Create a new position")
            .Accepts<AddPositionRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromBody] AddPositionRequest request,
        [FromServices] ICommandHandler<AddPositionCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
       var command = new AddPositionCommand(
            Name:            request.Name,
            Code:            request.Code,
            RequiresLicense: request.RequiresLicense
        );

        var result = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return Created($"~/positions/{result.Value}", result.Value);

        var error = result.Error!.Value;
        return error.Code switch
        {
            "validation" => ValidationProblem(
                errors: new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            "conflict"   => Conflict(new { message = error.Message }),
            "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _            => Problem(error.Message)
        };
    }
}