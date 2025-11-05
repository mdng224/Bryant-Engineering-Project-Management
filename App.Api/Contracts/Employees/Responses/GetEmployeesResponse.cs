namespace App.Api.Contracts.Employees.Responses;

public record GetEmployeesResponse(
    IReadOnlyList<EmployeeListItem> Employees,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);