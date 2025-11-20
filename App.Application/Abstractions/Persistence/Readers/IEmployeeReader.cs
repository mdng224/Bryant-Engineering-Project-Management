using App.Application.Common.Dtos;
using App.Application.Common.Dtos.Employees;
using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IEmployeeReader
{
    Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<EmployeeDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<EmployeeRowDto> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}