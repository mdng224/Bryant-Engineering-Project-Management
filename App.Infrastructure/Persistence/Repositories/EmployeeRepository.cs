using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeReader
{
    // --- Readers --------------------------------------------------------
    public async Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var employees = await db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CompanyEmail == normalizedEmail, ct);

        return employees;
    }

    public async Task<(IReadOnlyList<Employee> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        CancellationToken ct = default)
    {
        var query = db.Employees.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(normalizedNameFilter))
        {
            var pattern = $"%{normalizedNameFilter}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.LastName, pattern) ||
                EF.Functions.ILike(e.FirstName, pattern) ||
                EF.Functions.ILike(e.PreferredName!, pattern)
            );
        }

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        var employees = await query
            .OrderBy(e => e.LastName)
            .ThenBy(e => e.FirstName)
            .ThenBy(e => e.PreferredName == null)
            .ThenBy(e => e.PreferredName)
            .ThenBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (employees, totalCount);
    }
}