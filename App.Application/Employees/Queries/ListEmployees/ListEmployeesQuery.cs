using App.Application.Common.Pagination;

namespace App.Application.Employees.Queries.ListEmployees;

public record ListEmployeesQuery(PagedQuery PagedQuery, string? NameFilter, bool? IsDeleted);