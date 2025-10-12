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

    public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken ct = default)
        => await db.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default)
        => await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Email == email, ct);

    public Task<bool> ExistsByIdAsync(Guid userId, CancellationToken ct) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Id == userId, ct);
}
