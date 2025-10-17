namespace App.Application.Admins.Commands.PatchUser;

public enum PatchUserResult
{
    Ok,
    UserNotFound,
    RoleNotFound,
    NoChangesSpecified
}