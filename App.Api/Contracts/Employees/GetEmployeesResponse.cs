namespace App.Api.Contracts.Employees;

public record GetEmployeesResponse(
    IReadOnlyList<EmployeeListItem> Employees,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);