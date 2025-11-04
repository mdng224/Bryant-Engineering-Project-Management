using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IEmployeeReader
{
    Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<(IReadOnlyList<Employee> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        CancellationToken ct = default);
}