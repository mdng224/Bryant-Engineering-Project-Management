namespace App.Api.Contracts.Employees;

// name can be last, first, or nickname
public record GetEmployeesRequest(int Page, int PageSize, string? Name);
