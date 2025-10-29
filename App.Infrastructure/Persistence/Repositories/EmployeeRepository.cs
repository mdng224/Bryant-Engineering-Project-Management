using App.Application.Abstractions;
using App.Application.Abstractions.Persistence;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmployeeRepository(AppDbContext db) : IEmployeeReader
{
    // --- Readers --------------------------------------------------------
    public async Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        await db.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CompanyEmail == normalizedEmail, ct);
    
    public async Task<(IReadOnlyList<Employee> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedName = null,
        CancellationToken ct = default)
    {
        const int maxPageSize = 200;
        take = Math.Clamp(take, 1, maxPageSize);
        skip = Math.Max(0, skip);

        var query = db.Employees.AsNoTracking();

        // check last, first, and nickname
        if (!string.IsNullOrWhiteSpace(normalizedName))
        {
            var pattern = $"%{normalizedName.Trim()}%";
            query = query.Where(e =>
                EF.Functions.ILike(e.LastName, pattern) ||
                EF.Functions.ILike(e.FirstName, pattern) ||
                (e.PreferredName != null && EF.Functions.ILike(e.PreferredName, pattern))
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