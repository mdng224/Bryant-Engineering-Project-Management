using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class UserReader(AppDbContext db) : IUserReader
{
        public async Task<int> CountActiveAdminsAsync(CancellationToken ct = default)
    {
        var activeAdminCount =  await db.Users
            .AsNoTracking()
            .Where(u => u.DeletedAtUtc == null
                        && u.Status == UserStatus.Active
                        && u.RoleId == RoleIds.Administrator)
            .CountAsync(ct);

        return activeAdminCount;
    }

    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var userExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalizedEmail, ct);

        return userExists;
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Email == normalizedEmail, ct);

        return user;
    }

    public async Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Id == userId, ct);

        return user;
    }
    
    public async Task<User?> GetByIdIncludingDeletedAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users
            .IgnoreQueryFilters()
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return user;
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct)
    {
        var user = db.Users
            .AsTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return user;
    }

    public async Task<(IReadOnlyList<User> users, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? email = null,
        bool? isDeleted = null,
        CancellationToken ct = default)
    {
        var query = db.ReadSet<User>().ApplyDeletedFilter(isDeleted);

        if (!string.IsNullOrWhiteSpace(email))
        {
            var pattern = $"%{email.Trim()}%";
            query = query.Where(u => EF.Functions.ILike(u.Email, pattern));
        }

        var totalCount = await query.CountAsync(ct);
        if (totalCount == 0 || skip >= totalCount)
            return ([], totalCount);

        var users = await query
            .OrderBy(u => u.Email)
            .ThenBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (users, totalCount);
    }
}