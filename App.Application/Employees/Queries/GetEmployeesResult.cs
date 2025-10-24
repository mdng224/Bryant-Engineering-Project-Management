using App.Application.Common.Dtos;

namespace App.Application.Employees.Queries;

public sealed record GetEmployeesResult(
    IReadOnlyList<EmployeeDto> EmployeesDtos,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);