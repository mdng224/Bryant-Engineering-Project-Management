using App.Application.Abstractions;
using App.Application.Common;
using App.Domain.Security;
using static App.Application.Common.R;

namespace App.Application.Admins.Commands.UpdateUser;

public sealed class UpdateUserHandler(IUserWriter userWriter)
    : ICommandHandler<UpdateUserCommand, Result<UpdateUserResult>>
{
    public async Task<Result<UpdateUserResult>> Handle(UpdateUserCommand command, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(command.RoleName) && !command.IsActive.HasValue)
            return Ok(UpdateUserResult.NoChangesSpecified);

        // single tracked load → mutate → one save
        var user = await userWriter.GetForUpdateAsync(command.UserId, ct);
        if (user is null)
            return Fail<UpdateUserResult>("not_found", "User not found.");

        if (command.RoleName is { } roleName)   // rolename != null
        {
            if (!RoleIds.TryFromName(roleName.Trim(), out Guid roleId))
                return Fail<UpdateUserResult>("not_found", "Role not found.");
            user.SetRole(roleId);
        }

        if (command.IsActive is bool active)
            if (active) user.Activate();
            else        user.Deactivate();

        await userWriter.SaveChangesAsync(ct);

        return Ok(UpdateUserResult.Ok);
    }
}
