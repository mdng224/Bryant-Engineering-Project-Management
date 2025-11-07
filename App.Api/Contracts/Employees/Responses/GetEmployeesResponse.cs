namespace App.Api.Contracts.Employees.Responses;

public record GetEmployeesResponse(
    IReadOnlyList<EmployeeListItemResponse> Employees,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);