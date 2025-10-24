using App.Application.Abstractions;
using App.Application.Common;
using App.Application.Common.Mappers;
using App.Domain.Common;
using static App.Application.Common.R;

namespace App.Application.Employees.Queries;

public sealed class GetEmployeesHandler(IEmployeeReader reader)
    : IQueryHandler<GetEmployeesQuery, Result<GetEmployeesResult>>
{
    public async Task<Result<GetEmployeesResult>> Handle(GetEmployeesQuery query, CancellationToken ct)
    {
        var (page, pageSize, skip) = PagingDefaults.Normalize(query.Page, query.PageSize);
        var normalizedName = query.Name?.ToNormalizedName();
        
        var (employees, total) = await reader.GetPagedAsync(skip, pageSize, normalizedName, ct);

        var employeeDtos = employees.ToDto().ToList();
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);

        var getEmployeesResult = new GetEmployeesResult(employeeDtos, total, page, pageSize, totalPages);
        
        return Ok(getEmployeesResult);
    }
}