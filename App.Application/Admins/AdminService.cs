// App.Application.Admins/AdminService.cs
using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Security;

namespace App.Application.Admins;

public sealed class AdminService(IUserQueries userQueries, IUserCommands userCommands) : IAdminService
{
    public async Task<Result<AdminUpdateResult>> UpdateUserAsync(
        Guid userId, string? roleName, bool? isActive, CancellationToken ct)
    {
        var changeRole = !string.IsNullOrWhiteSpace(roleName);
        var changeStatus = isActive.HasValue;

        if (!changeRole && !changeStatus)
            return Result<AdminUpdateResult>.Success(AdminUpdateResult.NoChangesSpecified);

        // Ensure the user exists first (good UX for 404 vs 400).
        var exists = await userQueries.ExistsByIdAsync(userId, ct); // add ExistsByIdAsync to queries if not present
        if (!exists)
            return Result<AdminUpdateResult>.Success(AdminUpdateResult.UserNotFound);

        Guid? roleId = null;
        if (changeRole)
        {
            if (!RoleIds.TryFromName(roleName!.Trim(), out var rid))
                return Result<AdminUpdateResult>.Success(AdminUpdateResult.RoleNotFound);
            roleId = rid;
        }

        await userCommands.UpdateAsync(userId, roleId, isActive, ct);

        return Result<AdminUpdateResult>.Success(AdminUpdateResult.Ok);
    }
}