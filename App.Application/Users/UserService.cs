using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Security;

namespace App.Application.Users;

public sealed class UserService : IUserService
{
    private readonly IUserQueries _userQueries;
    private readonly IUserCommands _userCommands;

    public UserService(IUserQueries userQueries, IUserCommands userCommands)
        => (_userQueries, _userCommands) = (userQueries, userCommands);

    public async Task<Result<SetUserRoleResult>> SetUserRoleAsync(Guid userId, string roleName, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return Result<SetUserRoleResult>.Success(SetUserRoleResult.Invalid);

        // map exact strings to seeded GUIDs
        if (!RoleIds.TryFromName(roleName, out var roleId))
            return Result<SetUserRoleResult>.Success(SetUserRoleResult.RoleNotFound);

        var user = await _userQueries.GetByIdAsync(userId, ct);
        if (user is null)
            return Result<SetUserRoleResult>.Success(SetUserRoleResult.UserNotFound);

        await _userCommands.SetUserRoleAsync(user.Id, roleId, ct);

        return Result<SetUserRoleResult>.Success(SetUserRoleResult.Ok);
    }
}
