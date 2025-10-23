using App.Application.Abstractions;
using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserReader, IUserWriter
{
    // --- Readers --------------------------------------------------------
    public async Task<int> CountActiveAdminsAsync(CancellationToken ct = default) =>
        await db.Users
            .AsNoTracking()
            .Where(u => u.DeletedAtUtc == null
                        && u.Status == UserStatus.Active
                        && u.RoleId == RoleIds.Administrator)
            .CountAsync(ct);
    
    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().AnyAsync(u => u.Email == normalizedEmail, ct);

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default) =>
        await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Email == normalizedEmail, ct);

    public async Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.DeletedAtUtc == null && u.Id == userId, ct);

    public async Task<(IReadOnlyList<User> users, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? email = null,
        CancellationToken ct = default)
    {
        const int maxPageSize = 200;
        take = Math.Clamp(take, 1, maxPageSize);
        skip = Math.Max(0, skip);

        var query = db.Users.AsNoTracking();

        // Optional partial email match (case-insensitive, Postgres)
        if (!string.IsNullOrWhiteSpace(email))
        {
            var term = email.Trim();

            // Optional: escape user-provided wildcards so search is literal
            term = term.Replace("\\", @"\\").Replace("%", "\\%").Replace("_", "\\_");

            var pattern = $"%{term}%";
            query = query.Where(u => EF.Functions.ILike(u.Email, pattern));
        }

        var totalUsers = await query.CountAsync(ct);
        if (totalUsers == 0 || skip >= totalUsers)
            return ([], totalUsers);

        var users = await query
            .OrderBy(u => u.Email)
            .ThenBy(u => u.Id) // stable
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (users, totalUsers);
    }

    // --- Writers --------------------------------------------------------
    public async Task AddAsync(User user, CancellationToken ct)
    {
        await db.Users.AddAsync(user, ct);
        await db.SaveChangesAsync(ct);
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct); // tracked

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
    
    /// <summary>
    /// Updates a user entity without violating immutability constraints.
    /// 
    /// This method attaches the <see cref="User"/> entity to the DbContext if it isn't already tracked,
    /// then explicitly marks only the allowed fields as modified (e.g., Status).
    /// Immutable properties like Email are explicitly excluded from modification to prevent
    /// accidental updates and to comply with domain rules enforced in <see cref="AppDbContext.SaveChangesAsync"/>.
    /// 
    /// In other words:
    /// - It safely persists state transitions (e.g., verifying email, activating user)
    /// - It avoids EF’s default `Update()` behavior that marks all properties as modified
    /// - It ensures that immutable fields (like Email) remain unchanged in the database
    /// </summary>
    public async Task UpdateAsync(User user, CancellationToken ct = default)
    {
        var entry = db.Entry(user);
        if (entry.State == EntityState.Detached)
        {
            db.Users.Attach(user);
            entry = db.Entry(user);
        }
        
        // Only what Verify changed:
        entry.Property(u => u.Status).IsModified = true;
        // entry.Property(u => u.EmailVerifiedAtUtc).IsModified = true; // if you have it

        // Ensure immutables are not touched:
        entry.Property(u => u.Email).IsModified = false;
        
        await db.SaveChangesAsync(ct);
    }
}