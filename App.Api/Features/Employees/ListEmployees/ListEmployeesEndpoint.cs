using App.Application.Abstractions.Handlers;
using App.Application.Common.Dtos.Employees;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Queries;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.AspNetCore.Http.Results;

namespace App.Api.Features.Employees.ListEmployees;

public static class ListEmployeesEndpoint
{
    public static RouteGroupBuilder MapListEmployeesEndpoint(this RouteGroupBuilder group)
    {
        // GET /employees?page=&pageSize=
        group.MapGet("", Handle)
            .WithSummary("List all employees (paginated)")
            .Produces<ListEmployeesResponse>();

        return group;
    }
    private static async Task<IResult> Handle(
        [AsParameters] ListEmployeesRequest request,
        [FromServices] IQueryHandler<ListEmployeesQuery, Result<PagedResult<EmployeeRowDto>>> handler,
        CancellationToken ct = default)
    {
        var query = request.ToQuery();
        var result  = await handler.Handle(query, ct);
        if (!result.IsSuccess)
            return Problem(result.Error!.Value.Message);
        
        var dto = result.Value!;
        var response = dto.ToResponse();
        
        return Ok(response);
    }

    private static ListEmployeesQuery ToQuery(this ListEmployeesRequest request)
    {
        var pagedQuery = new PagedQuery(request.Page, request.PageSize);
        
        return new ListEmployeesQuery(pagedQuery, request.NameFilter, request.IsDeleted);
    }
    
    private static ListEmployeesResponse ToResponse(
        this PagedResult<EmployeeRowDto> pagedResult)
    {
        var employees = pagedResult.Items.Select(dto => dto.ToResponse()).ToList();

        return new ListEmployeesResponse(
            Employees:  employees,
            TotalCount: pagedResult.TotalCount,
            Page:       pagedResult.Page,
            PageSize:   pagedResult.PageSize,
            TotalPages: pagedResult.TotalPages
        );
    }
    
    private static EmployeeRowResponse ToResponse(this EmployeeRowDto dto) =>
        new(
            dto.Id,
            dto.LastName,
            dto.FirstName,
            dto.PreferredName,
            dto.PositionNames,
            dto.Department,
            dto.EmploymentType,
            dto.HireDate,
            dto.DeletedAtUtc
        );
}