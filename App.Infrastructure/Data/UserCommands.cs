using App.Application.Abstractions;
using App.Domain.Security;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserCommands(AppDbContext db) : IUserCommands
{
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

    public async Task SetUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        user.SetRole(roleId);

        await db.SaveChangesAsync(ct);
    }
}
