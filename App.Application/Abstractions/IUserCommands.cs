using App.Domain.Users;

namespace App.Application.Abstractions;

public interface IUserCommands
{
    Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default);
    Task SetUserRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
}
