using App.Application.Abstractions;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Data;

public class UserQueries(AppDbContext db) : IUserQueries
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email.ToLowerInvariant() == normalizedEmail, ct);

    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
        => await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email.ToLowerInvariant() == normalizedEmail, ct);
}
