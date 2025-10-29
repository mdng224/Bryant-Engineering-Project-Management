using App.Domain.Users;

namespace App.Application.Abstractions.Persistence;

public interface IUserWriter
{
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
