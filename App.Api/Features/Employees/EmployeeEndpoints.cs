using App.Api.Contracts.Employees;
using App.Api.Features.Employees.Mappers;
using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.EmployeePositions.Queries;
using App.Application.Employees.Queries;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees;

public static class EmployeeEndpoints
{
    public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
    {
        var employees = app.MapGroup("/employees")
            .WithTags("Employees");

        // GET /employees?page=&pageSize=
        employees.MapGet("", HandleGetEmployees)
            .WithSummary("List all employees (paginated)")
            .Produces<GetEmployeesResponse>();
    }

    private static async Task<IResult> HandleGetEmployees(
        [AsParameters] GetEmployeesRequest request,
        IQueryHandler<GetEmployeesQuery, Result<PagedResult<EmployeeDto>>> getEmployeesHandler,
        IQueryHandler<GetPositionsForEmployeesQuery, Result<IReadOnlyDictionary<Guid, IReadOnlyList<PositionMiniDto>>>>
            getPositionsForEmployeesHandler,
        CancellationToken ct = default)
    {
        // 1) Employees
        var getEmployeesQuery = request.ToQuery();
        var employeesResult  = await getEmployeesHandler.Handle(getEmployeesQuery, ct);
        if (!employeesResult.IsSuccess)
            return Problem(employeesResult.Error!.Value.Message);
        
        var employeesPage = employeesResult.Value!;
        var employeeIds = employeesPage.Items.Select(e => e.Id).Distinct().ToArray();
        
        // 2) Positions lookup (prefer a dedicated lookup query or cap the size)
        var getPositionsForEmployeesQuery = new GetPositionsForEmployeesQuery(employeeIds);
        var positionsMapResult = await getPositionsForEmployeesHandler.Handle(getPositionsForEmployeesQuery, ct);
        if (!positionsMapResult.IsSuccess)
            return Problem(positionsMapResult.Error!.Value.Message);
        
        var positionsByEmployee = positionsMapResult.Value!; // Dict<EmployeeId, List<PositionMiniDto>>
        
        // 3) Map to response
        var response = employeesResult.Value!.ToGetEmployeesResponse(positionsByEmployee);
        
        return Ok(response);
    }
}