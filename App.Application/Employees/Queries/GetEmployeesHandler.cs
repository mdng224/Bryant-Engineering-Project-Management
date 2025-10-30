using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Dtos;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Application.Employees.Mappers;
using App.Domain.Common;
using App.Domain.Employees;
using static App.Application.Common.R;

namespace App.Application.Employees.Queries;

public sealed class GetEmployeesHandler(IEmployeeReader reader)
    : IQueryHandler<GetEmployeesQuery, Result<PagedResult<EmployeeDto>>>
{
    public async Task<Result<PagedResult<EmployeeDto>>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        // Normalize pagination input
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);
        var normalizedName = query.Name?.ToNormalizedName();
        
        // Read data from repository
        var (employees, total) = await reader.GetPagedAsync(skip, pageSize, normalizedName, ct);

        // Map to DTOs and wrap in paged result
        var pagedResult = new PagedResult<Employee>(employees, total, page, pageSize)
            .Map(employee => employee.ToDto());
        
        return Ok(pagedResult);
    }
}