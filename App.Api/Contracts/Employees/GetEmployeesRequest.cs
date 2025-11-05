namespace App.Api.Contracts.Employees;

// Search name can be last, first, or nickname
public record GetEmployeesRequest(int Page, int PageSize, string? NameFilter, bool? IsDeleted);
