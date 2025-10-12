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

        if (exists)
            throw new InvalidOperationException("Email already exists.");

        var user = new User(email, passwordHash, RoleIds.User); // IsActive defaults (false) in domain

        db.Users.Add(user);

        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // In case of a race with the unique index on Email
            throw new InvalidOperationException("Email already exists.");
        }

        return user;
    }

    public async Task UpdateAsync(Guid userId, Guid? roleId, bool? isActive, CancellationToken ct)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
            ?? throw new InvalidOperationException("User not found.");

        if (roleId.HasValue)
            user.SetRole(roleId.Value);

        if (isActive.HasValue)
        {
            if (isActive.Value)
                user.Activate();
            else
                user.Deactivate();
        }

        await db.SaveChangesAsync(ct); // one transaction
    }
}