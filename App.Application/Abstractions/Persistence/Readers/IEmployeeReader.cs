using App.Application.Common.Dtos;
using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IEmployeeReader
{
    Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<(IReadOnlyList<EmployeeListItemDto> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}