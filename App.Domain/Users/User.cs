using App.Domain.Common;
using App.Domain.Employees;

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
    public Employee? Employee { get; private set; } = null!; // 1:1 back-nav
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
        Id = Guid.CreateVersion7();

        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        RoleId = Guard.AgainstDefault(roleId, nameof(roleId));

        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    // --- Mutators  -------------------------------------------------------------
    public void Activate()
    {
        EnsureNotDeleted();
        if (IsActive) return;
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        EnsureNotDeleted();
        if (!IsActive) return;
        IsActive = false;
        Touch();
    }
    
    public void SetRole(Guid newRoleId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(newRoleId, nameof(newRoleId));
        if (valid == RoleId) return;
        RoleId = valid;
        Touch();
    }

    public void SetPasswordHash(string passwordHash)
    {
        EnsureNotDeleted();
        var hash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        if (hash == PasswordHash) return;
        PasswordHash = hash;
        Touch();
    }

    public void SoftDelete()
    {
        if (DeletedAtUtc is not null) return;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DeletedAtUtc.Value;
    }

    public void Restore()
    {
        if (DeletedAtUtc is null) return;
        DeletedAtUtc = null;
        Touch();
    }

    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted user.");
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}
