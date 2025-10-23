using App.Domain.Employees;

namespace App.Application.Abstractions;

public interface IEmployeeReader
{
    Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default);
}