using App.Api.Common.Pagination;

namespace App.Api.Contracts.Employees;

// Search name can be last, first, or nickname
public record GetEmployeesRequest(PagedRequest PagedRequest, string? Name);
