using App.Application.Common.Dtos;
using App.Domain.Users;

namespace App.Application.Abstractions.Persistence.Readers;

public interface IUserReader
{
    Task<int> CountActiveAdminsAsync(CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string normalizedEmail, CancellationToken ct = default);
    Task<User?> GetActiveByIdAsync(Guid userId, CancellationToken ct = default);
    Task<(IReadOnlyList<UserDto> items, int totalCount)> GetPagedAsync(
        int skip,
        int take,
        string? email = null,
        bool? isDeleted = null,
        CancellationToken ct = default);
}
