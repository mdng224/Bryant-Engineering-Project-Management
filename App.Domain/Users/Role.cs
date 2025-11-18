using App.Domain.Common;

namespace App.Domain.Users;

public sealed class Role
{
    private Role() { }
    public Role(Guid id, string name)
    {
        Id = Guard.AgainstDefault(id, nameof(id));
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)).ToNormalizedName();
    }
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public ICollection<User> Users { get; } = [];
}