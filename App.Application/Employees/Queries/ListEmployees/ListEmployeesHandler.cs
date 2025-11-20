using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Employees;
using App.Application.Common.Pagination;
using App.Application.Common.Results;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Employees.Queries.ListEmployees;

public sealed class ListEmployeesHandler(IEmployeeReader reader)
    : IQueryHandler<ListEmployeesQuery, Result<PagedResult<EmployeeRowDto>>>
{
    public async Task<Result<PagedResult<EmployeeRowDto>>> Handle(ListEmployeesQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = query.PagedQuery;
        var normalizedNameFilter = query.NameFilter?.ToNormalizedName();

        var (items, total) = await reader.GetPagedAsync(skip,
            pageSize,
            normalizedNameFilter,
            query.IsDeleted,
            ct);

        var pagedResult = new PagedResult<EmployeeRowDto>(items, total, page, pageSize);
        
        return Ok(pagedResult);
    }
}