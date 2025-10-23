using App.Application.Abstractions;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeReader
{
    public async Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        await db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CompanyEmail == normalizedEmail, ct);
}