namespace App.Domain.Users;

public sealed class UserRole
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }

    // nav
    public User User { get; private set; } = null!;
    public Role Role { get; private set; } = null!;

    private UserRole() { }
    public UserRole(Guid userId, Guid roleId) => (UserId, RoleId) = (userId, roleId);
}