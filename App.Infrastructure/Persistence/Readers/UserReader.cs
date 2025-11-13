using App.Application.Abstractions.Persistence.Readers;
using App.Application.Common.Dtos;
using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class UserReader(AppDbContext db) : IUserReader
{
    public async Task<int> CountActiveAdminsAsync(CancellationToken ct = default)
    {
        var activeAdminCount =  await db.ReadSet<User>()
            .Where(u => u.DeletedAtUtc == null
                        && u.Status == UserStatus.Active
                        && u.RoleId == RoleIds.Administrator)
            .CountAsync(ct);

        return activeAdminCount;
    }

    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var userExists = await db.ReadSet<User>().AnyAsync(u => u.Email == normalizedEmail, ct);

        return userExists;
    }

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    {
        var user = await db.ReadSet<User>()
            .FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Email == normalizedEmail, ct);

        return user;
    }

    public async Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await db.ReadSet<User>()
            .FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Id == userId, ct);

        return user;
    }

    public async Task<(IReadOnlyList<UserDto> items, int totalCount)> GetPagedAsync(
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

        var items = await query
            .OrderBy(u => u.Email)
            .ThenBy(u => u.Id)
            .Skip(skip)
            .Take(take)
            .Select(u => new UserDto(
                u.Id,
                u.Email,
                u.Role.Name,
                u.Status,
                u.CreatedAtUtc,
                u.UpdatedAtUtc,
                u.DeletedAtUtc
                ))
            .ToListAsync(ct);

        return (items, totalCount);
    }
}