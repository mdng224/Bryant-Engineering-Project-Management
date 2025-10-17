using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Security;
using static App.Application.Common.R;

namespace App.Application.Admins.Commands.PatchUser;

public sealed class PatchUserHandler(IUserWriter userWriter)
    : ICommandHandler<PatchUserCommand, Result<PatchUserResult>>
{
    public async Task<Result<PatchUserResult>> Handle(PatchUserCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RoleName) && !command.IsActive.HasValue)
            return Ok(PatchUserResult.NoChangesSpecified);

        // single tracked load → mutate → one save
        var user = await userWriter.GetForPatchAsync(command.UserId, ct);
        if (user is null)
            return Fail<PatchUserResult>("not_found", "User not found.");

        if (command.RoleName is { } roleName)   // rolename != null
        {
            if (!RoleIds.TryFromName(roleName.Trim(), out Guid roleId))
                return Fail<PatchUserResult>("not_found", "Role not found.");
            user.SetRole(roleId);
        }

        if (command.IsActive is bool active)
            if (active) user.Activate();
            else        user.Deactivate();

        await userWriter.SaveChangesAsync(ct);

        return Ok(PatchUserResult.Ok);
    }
}
