using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence;

public interface IEmployeeReader
{
    Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<(IReadOnlyList<Employee> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedName = null,
        CancellationToken ct = default);
}