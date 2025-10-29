using App.Api.Contracts.Positions;
using App.Api.Features.Positions.Mappers;
using App.Api.Filters;
using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Positions.Commands.AddPosition;
using App.Application.Positions.Commands.DeletePosition;
using App.Application.Positions.Commands.UpdatePosition;
using App.Application.Positions.Queries.GetPositions;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Positions;

public static class PositionEndpoints
{
    public static void MapPositionEndpoints(this IEndpointRouteBuilder app)
    {
        var positions = app.MapGroup("/positions")
            .WithTags("Positions");

        // POST /positions
        positions.MapPost("", HandleAddPosition)
            .AddEndpointFilter<Validate<AddPositionRequest>>()
            .WithSummary("Create a new position")
            .Accepts<AddPositionRequest>("application/json")
            .Produces<PositionResponse>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict);
        
        // DELETE /positions/{id}
        positions.MapDelete("/{id:guid}", HandleDeletePosition)
            .WithSummary("Delete a position")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        
        // GET /positions?page=&pageSize=
        positions.MapGet("", HandleGetPositions)
            .WithSummary("List positions (paginated)")
            .Produces<GetPositionsResponse>();
        
        // PATCH /positions/{id}
        positions.MapPut("/{id:guid}", HandleUpdatePosition)
            .AddEndpointFilter<Validate<UpdatePositionRequest>>()
            .WithSummary("Update a position")
            .Accepts<UpdatePositionRequest>("application/json")
            .Produces<PositionResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest);
    }
    
    private static async Task<IResult> HandleAddPosition(
        [FromBody] AddPositionRequest request,
        ICommandHandler<AddPositionCommand, Result<PositionResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
        {
            var error = result.Error!.Value;
            return error.Code switch
            {
                "validation" => ValidationProblem(new Dictionary<string, string[]> { ["body"] = [error.Message] }),
                "conflict"   => Conflict(new { message = error.Message }), // e.g., duplicate Code
                "forbidden"  => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
                _            => Problem(error.Message)
            };
        }
        
        var response = result.Value!.ToResponse();

        return Created($"/positions/{response.Id}", response);
    }
    
    private static async Task<IResult> HandleDeletePosition(
        [FromRoute] Guid id,
        ICommandHandler<DeletePositionCommand, Result<Unit>> handler,
        CancellationToken ct)
    {
        var command = new DeletePositionCommand(id);
        var result = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();
        
        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "forbidden" => Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _ => Problem(error.Message)
        };
    }
    
    private static async Task<IResult> HandleGetPositions(
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
    
    private static async Task<IResult> HandleUpdatePosition(
        [FromRoute] Guid id,
        UpdatePositionRequest request,
        ICommandHandler<UpdatePositionCommand, Result<PositionResult>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand(id);
        var result = await handler.Handle(command, ct);

        if (!result.IsSuccess)
        {
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
        
        var response = result.Value!.ToResponse();

        return Ok(response);
    }
}