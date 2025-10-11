
using App.Application.Users;
using App.Application.Common;

namespace App.Application.Abstractions;

public interface IUserService
{
    Task<Result<SetUserRoleResult>> SetUserRoleAsync(Guid userId, string roleName, CancellationToken ct);
}
