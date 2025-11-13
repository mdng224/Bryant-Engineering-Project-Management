using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Employees;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class EmployeeReader(AppDbContext db) : IEmployeeReader
{
    public async Task<Employee?> GetByCompanyEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var employees = await db.ReadSet<Employee>()
            .FirstOrDefaultAsync(e => e.CompanyEmail == normalizedEmail, ct);

        return employees;
    }

    public async Task<(IReadOnlyList<EmployeeListItemDto> employees, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? normalizedNameFilter = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.ReadSet<Employee>().ApplyDeletedFilter(isDeleted);
        
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
            .Select(e => new EmployeeListItemDto(
                e.Id,
                e.UserId,
                e.FirstName,
                e.LastName,
                e.PreferredName,
                e.EmploymentType != null ? e.EmploymentType.ToString() : null,
                e.SalaryType     != null ? e.SalaryType.ToString()     : null,
                e.HireDate,
                e.EndDate,
                e.Department != null ? e.Department.ToString() : null,
                db.ReadSet<EmployeePosition>()
                    .Where(ep => ep.EmployeeId == e.Id && ep.Position.DeletedAtUtc == null)
                    .Select(ep => ep.Position.Name)
                    .ToList(),
                e.CompanyEmail,
                e.WorkLocation,
                e.Notes,
                e.RecommendedRoleId,
                e.IsPreapproved,
                e.CreatedAtUtc,
                e.UpdatedAtUtc,
                e.DeletedAtUtc
                ))
            .ToListAsync(ct);
        
        return (employees, totalCount);
    }
}