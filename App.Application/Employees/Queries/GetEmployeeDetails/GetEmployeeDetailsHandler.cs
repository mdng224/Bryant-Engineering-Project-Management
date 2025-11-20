using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Employees;
using App.Application.Common.Results;
using static App.Application.Common.R;

namespace App.Application.Employees.Queries.GetEmployeeDetails;

public class GetEmployeeDetailsHandler(IEmployeeReader reader)
    : IQueryHandler<GetEmployeeDetailsQuery, Result<EmployeeDetailsDto>>
{
    public async Task<Result<EmployeeDetailsDto>> Handle(GetEmployeeDetailsQuery query, CancellationToken ct)
    {
        var dto = await reader.GetDetailsAsync(query.Id, ct);

        return dto is null ? Fail<EmployeeDetailsDto>("not_found", "Employee not found.") : Ok(dto);
    }
}