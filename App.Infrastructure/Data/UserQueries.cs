using App.Application.Abstractions;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserQueries : IUserQueries
{
    private readonly AppDbContext _db;

    public UserQueries(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLowerInvariant();

        return await _db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == normalized, ct);
    }
}
