using App.Domain.Users;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IUserReader
{
    Task<int> CountActiveAdminsAsync(CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid userId, CancellationToken ct = default);
    Task<User?> GetForUpdateAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<User> users, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? email = null,
        CancellationToken ct = default);
}
