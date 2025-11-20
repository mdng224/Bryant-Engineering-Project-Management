using App.Api.Contracts.Employees.Responses;
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
            .Produces<EmployeeResponse>()
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

        if (!result.IsSuccess)
        {
            var error = result.Error!.Value;
            return error.Code switch
            {
                "not_found" => NotFound(new { message = error.Message }),
                "conflict"  => Conflict(new { message = error.Message }),   // employee taken
                "forbidden" => TypedResults.Json(new { message = error.Message }, statusCode: StatusCodes.Status403Forbidden),
                _           => Problem(error.Message)
            };
        }

        var response = result.Value!.ToResponse();
        
        return Ok(response);
    }

    private static EmployeeResponse ToResponse(this EmployeeListItemDto item) =>
        new(
            item.Id,
            item.UserId,
            item.FirstName,
            item.LastName,
            item.PreferredName,
            item.PositionNames,
            item.EmploymentType,
            item.SalaryType,
            item.HireDate,
            item.EndDate,
            item.Department,
            item.CompanyEmail,
            item.WorkLocation,
            item.Notes,
            item.RecommendedRoleId,
            item.IsPreapproved,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            item.DeletedAtUtc
        );
}