using App.Application.Abstractions;
using App.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserReader, IUserWriter
{
    // Readers
    public async Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    => await db.Users
        .AsNoTracking()
        .AnyAsync(u => u.Email == normalizedEmail, ct);

    public Task<bool> ExistsByIdAsync(Guid userId, CancellationToken ct) =>
        db.Users.AsNoTracking().AnyAsync(u => u.Id == userId, ct);

    public async Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default)
    => await db.Users
        .AsNoTracking()
        .Include(u => u.Role)
        .FirstOrDefaultAsync(u => u.Email == normalizedEmail, ct);

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<(IReadOnlyList<User> users, int totalCount)> GetPagedAsync(
        int skip, int take, CancellationToken ct = default)
    {
        const int MaxPageSize = 200;
        take = Math.Clamp(take, 1, MaxPageSize);
        skip = Math.Max(0, skip);

        var query = db.Users.AsNoTracking();

        var total = await query.CountAsync(ct);
        if (total == 0 || skip >= total)
            return ([], total);

        var items = await query
            .OrderBy(u => u.Email).ThenBy(u => u.Id) // stable
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return (items, total);
    }

    // Writers
    public Task AddAsync(User user, CancellationToken ct)
    {
        db.Users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == id, ct); // tracked

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}