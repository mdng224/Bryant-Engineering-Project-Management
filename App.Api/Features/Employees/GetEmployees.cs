using App.Api.Contracts.Employees;
using App.Api.Mappers.Employees;
using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Employees.Queries;
using App.Application.Positions.Queries;
using App.Application.Positions.Queries.GetPositions;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees;

public static class GetEmployees
{
    public static async Task<IResult> Handle(
        [AsParameters] GetEmployeesRequest request,
        IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>> getEmployeesHandler,
        IQueryHandler<GetPositionsQuery, Result<GetPositionsResult>> getPositionsHandler,
        CancellationToken ct = default)
    {
        var getEmployeesQuery = request.ToQuery();
        
        var employeesResult  = await getEmployeesHandler.Handle(getEmployeesQuery, ct);
        if (!employeesResult.IsSuccess)
            return Problem(employeesResult.Error!.Value.Message);
        
        var positionsResult = await getPositionsHandler.Handle(new GetPositionsQuery(1,  int.MaxValue), ct);
        if (!positionsResult.IsSuccess)
            return Problem(positionsResult.Error!.Value.Message);
        
        var positionLookup = positionsResult.Value!.Positions
            .ToDictionary(p => p.Id, p => p.Name);

        var response = employeesResult.Value!.ToGetEmployeesResponse(positionLookup);
        
        return Ok(response);
    }
}