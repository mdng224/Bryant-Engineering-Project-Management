using App.Application.Abstractions;
using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserCommands(AppDbContext db) : IUserCommands
{
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

    public async Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        var exists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);

        if (exists) throw new InvalidOperationException("Email already exists.");

        var user = new User(email, passwordHash, RoleIds.User);

        db.Users.Add(user);
        await db.SaveChangesAsync(ct);

        return user;
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

    public async Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new KeyNotFoundException("User not found.");

        user.SetPasswordHash(hash);

        await db.SaveChangesAsync(ct);
    }
}
