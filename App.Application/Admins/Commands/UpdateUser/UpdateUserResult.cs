namespace App.Application.Admins.Commands.UpdateUser;

public enum UpdateUserResult
{
    Ok,
    UserNotFound,
    RoleNotFound,
    NoChangesSpecified
}