using App.Domain.Users;

namespace App.Application.Abstractions.Persistence.Writers;

public interface IUserWriter
{
    void Add(User user);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
    Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct);
}
