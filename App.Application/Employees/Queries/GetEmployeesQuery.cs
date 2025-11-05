using App.Application.Common.Pagination;

namespace App.Application.Employees.Queries;

public record GetEmployeesQuery(PagedQuery PagedQuery, string? NameFilter, bool? IsDeleted);