using App.Domain.Users;

namespace App.Application.Abstractions;

public interface IUserCommands
{
    Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default);
    Task UpdateAsync(Guid userId, Guid? roleId, bool? isActive, CancellationToken ct);
}
