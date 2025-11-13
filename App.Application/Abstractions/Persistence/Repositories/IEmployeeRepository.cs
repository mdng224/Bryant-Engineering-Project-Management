using App.Domain.Employees;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetAsync(Guid id, CancellationToken ct = default);
}