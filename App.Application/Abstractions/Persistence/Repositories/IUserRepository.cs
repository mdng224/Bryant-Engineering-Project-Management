using App.Domain.Users;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IUserRepository
{
    void Add(User user);
    Task<bool> SoftDeleteAsync(Guid id, CancellationToken ct);
}
