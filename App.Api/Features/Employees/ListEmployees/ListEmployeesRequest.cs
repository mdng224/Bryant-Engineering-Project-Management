namespace App.Api.Features.Employees.ListEmployees;

public record ListEmployeesRequest(
    int Page,
    int PageSize,
    string? NameFilter,
    bool? IsDeleted
);
