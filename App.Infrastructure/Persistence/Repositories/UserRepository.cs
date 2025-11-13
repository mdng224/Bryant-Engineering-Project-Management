using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Common.Abstractions;
using App.Domain.Users;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(AppDbContext db) : IUserRepository
{
    public void Add(User user) => db.Users.Add(user);

    public async Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct = default)
    {
        var user = await db.Users.FindAsync([id], ct);

        switch (user)
        {
            case null:
                return false;
            case ISoftDeletable { DeletedAtUtc: not null }:
                return true;
            default:
                db.Users.Remove(user); // interceptor flips to soft-delete
                return true;
        }
    }


}