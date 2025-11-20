using App.Api.Contracts.Employees.Requests;
using App.Api.Contracts.Employees.Responses;
using App.Api.Features.Employees.Mappers;
using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Commands.AddEmployee;
using App.Application.Employees.Commands.RestoreEmployee;
using App.Application.Employees.Queries;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees;

public static class EmployeeModule
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/employees")
            .WithTags("Employees");
        
        // POST /employees
        employees.MapPost("", HandleAddEmployee)
            .AddEndpointFilter<Validate<AddEmployeeRequest>>()
            .WithSummary("Create a new employee")
            .Accepts<AddEmployeeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);
        
        // GET /employees?page=&pageSize=
        employees.MapGet("", HandleGetEmployees)
            .WithSummary("List all employees (paginated)")
            .Produces<GetEmployeesResponse>();
        
        // POST /employees/{id}/restore
        employees.MapPost("/{id:guid}/restore", HandleRestoreEmployee)
            .WithSummary("Restore a soft-deleted employee")
            .Produces<EmployeeResponse>()
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> HandleAddEmployee(
        [FromBody] AddEmployeeRequest request,
        [FromServices] ICommandHandler<AddEmployeeCommand, Result<Guid>> handler,
        CancellationToken ct)
    {
        var command = request.ToCommand();
        var result  = await handler.Handle(command, ct);

        if (result.IsSuccess)
            return Created($"~/employees/{result.Value}", result.Value);

        var error = result.Error!.Value;
        return error.Code switch
        {
            "validation" => ValidationProblem(
                errors: new Dictionary<string, string[]> { ["body"] = [error.Message] }),
            "conflict"   => Conflict(new { message = error.Message }),
            "forbidden"  => Json(
                new { message = error.Message },
                statusCode: StatusCodes.Status403Forbidden),
            _            => Problem(error.Message)
        };
    }
    
    private static async Task<IResult> HandleGetEmployees(
        [AsParameters] GetEmployeesRequest request,
        [FromServices] IQueryHandler<GetEmployeesQuery, Result<PagedResult<EmployeeListItemDto>>> handler,
        CancellationToken ct = default)
    {
        var query = request.ToQuery();
        var result  = await handler.Handle(query, ct);
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);
        
        var response = result.Value!.ToGetEmployeesResponse();
        
        return Ok(response);
    }
    
    private static async Task<IResult> HandleRestoreEmployee(
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

        var response = result.Value!.ToEmployeeResponse();
        
        return Ok(response);
    }
}