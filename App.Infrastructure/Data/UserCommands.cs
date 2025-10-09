using App.Application.Abstractions;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserCommands(AppDbContext db) : IUserCommands
{
    public async Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        // normalize for lookup only (DB has unique index on lower("Email"))
        var normalized = (email ?? string.Empty).Trim().ToLowerInvariant();

        var exists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLowerInvariant() == normalized, ct);

        if (exists)
            throw new InvalidOperationException("Email already exists.");

        // store trimmed original casing
        var user = new User((email ?? string.Empty).Trim(), passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return user;
    }

    public async Task UpdateEmailAsync(Guid userId, string newEmail, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        var trimmed = (newEmail ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            throw new ArgumentException("Email cannot be empty.", nameof(newEmail));

        var normalized = trimmed.ToLowerInvariant();

        // conflict check against normalized value
        var conflict = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id != userId && u.Email.ToLowerInvariant() == normalized, ct);

        if (conflict)
            throw new InvalidOperationException("Email already exists.");

        // assign via EF to respect private setters on the entity
        db.Entry(user).Property(nameof(User.Email)).CurrentValue = trimmed;
        db.Entry(user).Property(nameof(User.UpdatedAtUtc)).CurrentValue = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
                   ?? throw new KeyNotFoundException("User not found.");

        if (string.IsNullOrWhiteSpace(hash))
            throw new ArgumentException("Password hash cannot be empty.", nameof(hash));

        db.Entry(user).Property(nameof(User.PasswordHash)).CurrentValue = hash;
        db.Entry(user).Property(nameof(User.UpdatedAtUtc)).CurrentValue = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);
    }

    public async Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var exists = await db.Set<UserRole>()
            .AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        if (!exists)
        {
            db.Set<UserRole>().Add(new UserRole(userId, roleId));
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task RemoveUserFromRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var link = await db.Set<UserRole>()
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);

        if (link is not null)
        {
            db.Remove(link);
            await db.SaveChangesAsync(ct);
        }
    }
}
