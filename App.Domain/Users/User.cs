using App.Domain.Common;

namespace App.Domain.Users;

/// <summary>
/// Represents an application user with authentication credentials.
/// Implements <see cref="IAuditableEntity"/> to support audit tracking.
/// </summary>
public class User : IAuditableEntity
{
    // --- Key ------------------------------------------------------------------
    public Guid Id { get; private set; }

    // --- Core Fields ----------------------------------------------------------

    public string Email { get; private set; } = null!;  // Normalized (trimmed) on creation.
    public string PasswordHash { get; private set; } = null!;  // Secure password hash (BCrypt). Never stored in plain text.
    public Role Role { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public bool IsActive { get; private set; }


    // TODO: Point to a client when that domain is ready.
    // TODO: Point to an employee when that domain is ready.

    // --- Auditing ------------------------------------------------------------

    /// <inheritdoc/>
    public DateTimeOffset CreatedAtUtc { get; private set; }
    /// <inheritdoc/>
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    /// <inheritdoc/>
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // --- Constructors --------------------------------------------------------
    private User() { }

    public User(string email, string passwordHash, Guid roleId)
    {
        Id = Guid.CreateVersion7();            // ordered, time-based GUID (requires .NET 8+)

        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        RoleId = roleId;

        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    // --- Mutators  -------------------------------------------------------------

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
    public void SetRole(Guid newRoleId)
    {
        EnsureNotDeleted();

        Guard.AgainstDefault(newRoleId, nameof(newRoleId));
        if (newRoleId == RoleId) return; // no-op

        RoleId = newRoleId;
        Touch();
    }

    public void SetPasswordHash(string passwordHash)
    {
        EnsureNotDeleted();

        var hash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        if (hash == PasswordHash) return; // no-operation

        PasswordHash = hash;
        Touch();
    }

    public void SoftDelete()
    {
        if (DeletedAtUtc is null)
        {
            DeletedAtUtc = DateTimeOffset.UtcNow;
            UpdatedAtUtc = DeletedAtUtc.Value;
        }
    }

    public void Restore()
    {
        if (DeletedAtUtc is not null)
        {
            DeletedAtUtc = null;
            Touch();
        }
    }

    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted user.");
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
