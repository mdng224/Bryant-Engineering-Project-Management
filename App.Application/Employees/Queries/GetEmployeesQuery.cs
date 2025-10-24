namespace App.Application.Employees.Queries;

public record GetEmployeesQuery(int Page, int PageSize, string? Name);