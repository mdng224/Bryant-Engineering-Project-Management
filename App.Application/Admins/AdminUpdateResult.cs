namespace App.Application.Admins;

public enum AdminUpdateResult
{
    Ok,
    UserNotFound,
    RoleNotFound,
    NoChangesSpecified
}