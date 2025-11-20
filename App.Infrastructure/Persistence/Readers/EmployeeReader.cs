using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos.Employees;
using App.Domain.Employees;
using App.Domain.Users;
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

    public async Task<EmployeeDetailsDto?> GetDetailsAsync(Guid id, CancellationToken ct = default)
    {
        var employeeDetails = await db.ReadSet<Employee>()
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDetailsDto(
                e.Id,
                e.UserId,
                // Identity
                e.FirstName,
                e.LastName,
                e.PreferredName,
                // Employment
                e.EmploymentType.HasValue ? e.EmploymentType.ToString() : null,
                e.SalaryType.HasValue     ? e.SalaryType.ToString()     : null,
                e.HireDate,
                e.EndDate,
                // Organization
                e.Department.HasValue ? e.Department.ToString() : null,
                e.Positions
                    .Where(ep => ep.Position.DeletedAtUtc == null)
                    .OrderBy(ep => ep.Position.Name)
                    .Select(ep => ep.Position.Name)
                    .ToList(),
                // Contact / Misc
                e.CompanyEmail,
                e.WorkLocation,
                e.Notes,
                // Address
                e.Address != null ? e.Address.Line1      : null,
                e.Address != null ? e.Address.Line2      : null,
                e.Address != null ? e.Address.City       : null,
                e.Address != null ? e.Address.State      : null,
                e.Address != null ? e.Address.PostalCode : null,
                // Recommendation / preapproval
                db.ReadSet<Role>()
                    .Where(r => r.Id == e.RecommendedRoleId)
                    .Select(r => r.Name)
                    .FirstOrDefault(),
                e.IsPreapproved,
                // Auditing
                e.CreatedAtUtc,
                e.UpdatedAtUtc,
                e.DeletedAtUtc,
                e.CreatedById,
                e.UpdatedById,
                e.DeletedById
            ))
            .SingleOrDefaultAsync(ct);

        return employeeDetails;
    }
    
    public async Task<(IReadOnlyList<EmployeeRowDto> employees, int totalCount)> GetPagedAsync(
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
            .Select(e => new EmployeeRowDto(
                e.Id,
                e.LastName,
                e.FirstName,
                e.PreferredName,
                db.ReadSet<EmployeePosition>()
                    .Where(ep => ep.EmployeeId == e.Id && ep.Position.DeletedAtUtc == null)
                    .Select(ep => ep.Position.Name)
                    .ToList(),
                e.EmploymentType != null ? e.EmploymentType.ToString() : null,
                e.SalaryType     != null ? e.SalaryType.ToString()     : null,
                e.HireDate,
                e.DeletedAtUtc
                ))
            .ToListAsync(ct);
        
        return (employees, totalCount);
    }
}