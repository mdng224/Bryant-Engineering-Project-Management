using App.Domain.Users;

namespace App.Application.Abstractions;
public interface IUserCommands
{
    Task AddUserToRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task<User> CreateAsync(string email, string passwordHash, CancellationToken ct = default);
    Task RemoveUserFromRoleAsync(Guid userId, Guid roleId, CancellationToken ct = default);
    Task UpdatePasswordHashAsync(Guid userId, string hash, CancellationToken ct = default);
}
