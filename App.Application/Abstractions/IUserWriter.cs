using App.Domain.Users;

namespace App.Application.Abstractions;

public interface IUserWriter
{
    Task AddAsync(User user, CancellationToken ct);
    Task<User?> GetForPatchAsync(Guid id, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
