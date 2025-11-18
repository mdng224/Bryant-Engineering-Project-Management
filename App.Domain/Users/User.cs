using App.Domain.Common;
using App.Domain.Common.Abstractions;
using App.Domain.Employees;

namespace App.Domain.Users;

/// <summary>Application user with credentials; audited and soft-deletable.</summary>
public sealed class User : IAuditableEntity, ISoftDeletable
{
    private User() { }
    public User(string email, string passwordHash, Guid roleId)
    {
        Id = Guid.CreateVersion7();
        Email = Guard.AgainstNullOrWhiteSpace(email, nameof(email)).ToNormalizedEmail();
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        RoleId = Guard.AgainstDefault(roleId, nameof(roleId));
        Status = UserStatus.PendingEmail;
    }
    public Guid Id { get; private set; }
    public Employee? Employee { get; private set; } = null!; // 1:1 back-nav
    public string Email { get; private set; } = null!;  // Normalized (trimmed) on creation.
    public string PasswordHash { get; private set; } = null!;  // Secure password hash (BCrypt). Never stored in plain text.
    public Role Role { get; private set; } = null!;
    public Guid RoleId { get; private set; }
    public DateTimeOffset? EmailVerifiedAt { get; private set; }
    public UserStatus Status { get; private set; } = UserStatus.PendingEmail;

    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;


    // --- Mutators  -------------------------------------------------------------
    public void Activate()
    {
        EnsureNotDeleted();
        if (Status is UserStatus.Denied or UserStatus.Disabled)
            throw new InvalidOperationException("Cannot activate a denied or disabled user.");
        if (EmailVerifiedAt is null)
            throw new InvalidOperationException("Cannot activate before email verification.");
        Status = UserStatus.Active;
    }

    public void MarkPendingApproval()
    {
        EnsureNotDeleted();
        if (Status == UserStatus.PendingApproval)
            return;
        
        if (EmailVerifiedAt is null)
            throw new InvalidOperationException("Cannot mark pending approval before email verification.");
        Status = UserStatus.PendingApproval;
    }
    
    public void Deny()
    {
        EnsureNotDeleted();
        if (Status == UserStatus.Denied)
            return;
        
        if (Status != UserStatus.PendingApproval)
            throw new InvalidOperationException("Can only deny from PendingApproval.");
        Status = UserStatus.Denied;
    }
    
    public void MarkEmailVerified()
    {
        EnsureNotDeleted();
        if (EmailVerifiedAt is not null)
            return;
        
        EmailVerifiedAt = DateTimeOffset.UtcNow;
    }
    
    public void SetRole(Guid newRoleId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(newRoleId, nameof(newRoleId));
        if (valid == RoleId) return;
        RoleId = valid;
    }
    
    public void SetPasswordHash(string passwordHash)
    {
        EnsureNotDeleted();
        var hash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));
        if (hash == PasswordHash) return;
        PasswordHash = hash;
    }
    
    public void SetStatus(UserStatus status)
    {
        EnsureNotDeleted();
        if (Status == status)
            return;

        switch (status)
        {
            case UserStatus.PendingEmail:
            case UserStatus.PendingApproval:
            case UserStatus.Denied:
                Status = status;
                break;
            case UserStatus.Active:
                Activate();  // enforces rules
                break;
            case UserStatus.Disabled:
                Disable();   // already enforces domain rules
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "Unsupported user status transition.");
        }
    }
    
    // Optional convenience: undo soft-delete (interceptor will bump Updated*)
    public bool Restore()
    {
        if (!IsDeleted)
            return false;
        
        DeletedAtUtc = null;
        DeletedById = null;
        return true;
    }

    // --- Helpers --------------------------------------------------------------
    private void Disable()
    {
        EnsureNotDeleted();
        Status = UserStatus.Disabled;
    }
    
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted user.");
    }
}
