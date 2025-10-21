namespace App.Domain.Users;

public enum UserStatus
{
    PendingEmail,
    PendingApproval,
    Active,
    Denied,
    Disabled
}