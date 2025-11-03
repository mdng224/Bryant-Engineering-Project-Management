using App.Application.Abstractions;
using App.Application.Abstractions.Handlers;
using App.Application.Abstractions.Persistence;
using App.Application.Common;
using App.Application.Common.Results;
using App.Domain.Security;
using App.Domain.Users;
using static App.Application.Common.R;

namespace App.Application.Users.Commands.UpdateUser;

public sealed class UpdateUserHandler(IUserReader userReader, IUserWriter userWriter)
    : ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    public async Task<Result<UpdateUserResult>> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        if (IsNoOp(command)) return Ok(UpdateUserResult.NoChangesSpecified);

        var user = await userWriter.GetForUpdateAsync(command.UserId, ct);
        if (user is null) return Fail<UpdateUserResult>("not_found", "User not found.");
        
        // Figure out intended change
        var intent = ComputeIntent(user, command);
        if (!intent.Success) return Fail<UpdateUserResult>("not_found", "Role not found.");

        // Guard: don’t remove/deactivate the last active admin
        var guard = await EnsureNotRemovingLastAdminAsync(user, intent, ct);
        if (!guard.IsSuccess) return Fail<UpdateUserResult>(guard.Error!.Value.Code, guard.Error.Value.Message);

        // Apply changes
        ApplyRoleChange(user, intent);
        ApplyStatusChange(user, command);

        await userWriter.SaveChangesAsync(ct);

        return Ok(UpdateUserResult.Ok);
    }
    
    // ---------- Helpers ----------
    private static void ApplyStatusChange(User user, UpdateUserCommand cmd)
    {
        if (cmd.Status is not { } status) return;
        user.SetStatus(status);
    }
    
    private static void ApplyRoleChange(User user, Intent intent)
    {
        if (intent is { IsRoleChange: true, NewRoleId: { } roleId })
            user.SetRole(roleId);
    }
    
    private static Intent ComputeIntent(User user, UpdateUserCommand cmd)
    {
        Guid? newRoleId = null;
        var isRoleChange = false;

        if (cmd.RoleName is { } rn)
        {
            if (!RoleIds.TryFromName(rn.Trim(), out var resolved))
                return Intent.Failure;

            newRoleId = resolved;
            isRoleChange = true;
        }

        var isStatusChange = cmd.Status.HasValue;
        var isDeactivation = cmd.Status == UserStatus.Disabled;

        var isRoleDemotion =
            isRoleChange &&
            user.RoleId == RoleIds.Administrator &&
            newRoleId != RoleIds.Administrator;
        
        return new Intent(
            Success: true,
            IsRoleChange: isRoleChange,
            NewRoleId: newRoleId,
            IsStatusChange: isStatusChange,
            IsDeactivation: isDeactivation,
            IsRoleDemotion: isRoleDemotion
        );
    }
    
    private async Task<Result> EnsureNotRemovingLastAdminAsync(User user, Intent intent, CancellationToken ct)
    {
        if (!IsCurrentlyActiveAdmin(user) || !(intent.IsDeactivation || intent.IsRoleDemotion))
            return Result.Success();

        var activeAdminCount = await userReader.CountActiveAdminsAsync(ct);

        return activeAdminCount == 1
            ? Result.Fail("forbidden", "Cannot remove or deactivate the last active administrator.")
            : Result.Success();
    }
    
    private static bool IsCurrentlyActiveAdmin(User user) =>
        user.Status == UserStatus.Active && user.RoleId == RoleIds.Administrator;
    
    private static bool IsNoOp(UpdateUserCommand cmd) =>
        string.IsNullOrWhiteSpace(cmd.RoleName) && !cmd.Status.HasValue;
    
    private readonly record struct Intent(
        bool Success,
        bool IsRoleChange,
        Guid? NewRoleId,
        bool IsStatusChange,
        bool IsDeactivation,
        bool IsRoleDemotion
    )
    {
        public static Intent Failure =>
            new(
                Success: false,
                IsRoleChange: false,
                NewRoleId: null,
                IsStatusChange: false,
                IsDeactivation: false,
                IsRoleDemotion: false
            );
    }
}
