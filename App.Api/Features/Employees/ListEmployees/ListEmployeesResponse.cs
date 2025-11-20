namespace App.Api.Features.Employees.ListEmployees;

public record ListEmployeesResponse(
    IReadOnlyList<EmployeeRowResponse> Employees,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);