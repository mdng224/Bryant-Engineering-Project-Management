using App.Domain.Users;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    void Add(User user);
    Task<User?> GetAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct = default);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
}
