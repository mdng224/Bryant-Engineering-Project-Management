namespace App.Application.Users.Commands.UpdateUser;

public enum UpdateUserResult
{
    Ok,
    UserNotFound,
    RoleNotFound,
    NoChangesSpecified
}