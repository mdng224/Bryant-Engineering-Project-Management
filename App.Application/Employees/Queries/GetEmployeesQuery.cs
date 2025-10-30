using App.Application.Common.Abstractions;

namespace App.Application.Employees.Queries;

public record GetEmployeesQuery(int Page, int PageSize, string? Name) : IPagedQuery;