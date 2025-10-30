using App.Api.Contracts.Employees;
using App.Api.Features.Employees.Mappers;
using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Queries;
using App.Application.Positions.Queries.GetPositions;
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
        IQueryHandler<GetPositionsQuery, Result<PagedResult<PositionDto>>> getPositionsHandler,
        CancellationToken ct = default)
    {
        // 1) Employees
        var getEmployeesQuery = request.ToQuery();
        var employeesResult  = await getEmployeesHandler.Handle(getEmployeesQuery, ct);
        if (!employeesResult.IsSuccess)
            return Problem(employeesResult.Error!.Value.Message);
        
        // 2) Positions lookup (prefer a dedicated lookup query or cap the size)
        // TODO: Make a dedicated position lookup without pagination
        var pagedQuery = new PagedQuery(1, int.MaxValue);
        var positionsResult = await getPositionsHandler.Handle(new GetPositionsQuery(pagedQuery), ct);
        if (!positionsResult.IsSuccess)
            return Problem(positionsResult.Error!.Value.Message);
        
        var positionLookup = positionsResult.Value!.Items
            .ToDictionary(p => p.Id, p => p.Name);
        
        // 3) Map to response
        var response = employeesResult.Value!.ToGetEmployeesResponse(positionLookup);
        return Ok(response);
    }
}