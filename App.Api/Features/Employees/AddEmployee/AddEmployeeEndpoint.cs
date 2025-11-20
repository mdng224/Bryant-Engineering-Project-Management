using App.Api.Filters;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Results;
using App.Application.Employees.Commands.AddEmployee;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees.AddEmployee;

public static class AddEmployeeEndpoint
{
    public static RouteGroupBuilder MapAddEmployeeEndpoint(this RouteGroupBuilder group)
    {
        // POST /employees
        group.MapPost("", Handle)
            .AddEndpointFilter<Validate<AddEmployeeRequest>>()
            .WithSummary("Create a new employee")
            .Accepts<AddEmployeeRequest>("application/json")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status409Conflict)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }
    
    private static async Task<IResult> Handle(
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

    private static AddEmployeeCommand ToCommand(this AddEmployeeRequest request) =>
        new(
            FirstName          : request.FirstName,
            LastName           : request.LastName,
            PreferredName      : request.PreferredName,
            UserId             : request.UserId,
            EmploymentType     : request.EmploymentType,
            SalaryType         : request.SalaryType,
            Department         : request.Department,
            HireDate           : request.HireDate,
            CompanyEmail       : request.CompanyEmail,
            WorkLocation       : request.WorkLocation,
            Notes              : request.Notes,
            AddressLine1       : request.AddressLine1,
            AddressLine2       : request.AddressLine2,
            City               : request.City,
            State              : request.State,
            PostalCode         : request.PostalCode,
            RecommendedRoleId  : request.RecommendedRoleId
        );
}