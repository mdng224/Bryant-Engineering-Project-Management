using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Results;
using App.Application.Employees.Commands.RestoreEmployee;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees.RestoreEmployee;

public static class RestoreEmployeeEndpoint
{
    public static RouteGroupBuilder MapRestoreEmployeeEndpoint(this RouteGroupBuilder group)
    {
        // POST /employees/{id}/restore
        group.MapPost("/{id:guid}/restore", Handle)
            .WithSummary("Restore a soft-deleted employee")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);

        return group;
    }
    
    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        [FromServices] ICommandHandler<RestoreEmployeeCommand, Result<EmployeeListItemDto>> handler,
        CancellationToken ct)
    {
        var command = new RestoreEmployeeCommand(id);
        var result = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return NoContent();
        
        var error = result.Error!.Value;
        return error.Code switch
        {
            "not_found" => NotFound(new { message = error.Message }),
            "conflict"  => Conflict(new { message = error.Message }),   // unique-name/code taken
            "forbidden" => TypedResults.Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
            _           => Problem(error.Message)
        };
    }
}