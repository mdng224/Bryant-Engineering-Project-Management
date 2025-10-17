using App.Domain.Users;

namespace App.Application.Abstractions;

public interface IUserReader
{
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<(IReadOnlyList<User> users, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? email = null,
        CancellationToken ct = default);
}
