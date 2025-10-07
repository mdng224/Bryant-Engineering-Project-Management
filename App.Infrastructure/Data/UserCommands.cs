using App.Application.Abstractions;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserCommands : IUserCommands
{
    private readonly AppDbContext _db;

    public UserCommands(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default)
    {
        if (await _db.Users.AnyAsync(u => u.Email == email, ct))
            throw new InvalidOperationException("Email already exists.");

        var user = new User(email, passwordHash);
        _db.Users.Add(user);
        await _db.SaveChangesAsync(ct);

        return user;
    }

    public async Task UpdateEmailAsync(Guid userId, string newEmail, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
           ?? throw new KeyNotFoundException("User not found.");

        user.SetEmail(newEmail);

        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct)
           ?? throw new KeyNotFoundException("User not found.");

        user.SetPasswordHash(hash);

        await _db.SaveChangesAsync(ct);
    }
}
