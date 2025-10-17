using App.Domain.Common;

namespace App.Domain.Users;

public sealed class Role
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------
    public string Name { get; private set; } = null!;
    public ICollection<User> Users { get; } = [];

    // --- Constructors --------------------------------------------------------
    // ReSharper disable once UnusedMember.Local
    private Role() { }
    public Role(Guid id, string name)
    {
        Id = id;
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)).ToNormalizedName();
    }
}