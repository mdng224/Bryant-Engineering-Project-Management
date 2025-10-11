namespace App.Domain.Security;

public static class RoleNames
{
    public const string Administrator   = "Administrator";
    public const string Manager         = "Manager";
    public const string User            = "User";

    public static readonly string[] All = [Administrator, Manager, User];
}
