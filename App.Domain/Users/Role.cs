namespace App.Domain.Users;

public sealed class Role
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!; // "Administrator", "ProjectManager", "User"

    // nav
    public ICollection<UserRole> UserRoles { get; } = [];

    private Role() { }
    public Role(Guid id, string name) => (Id, Name) = (id, name);
}