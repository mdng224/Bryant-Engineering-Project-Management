using App.Domain.Common;

namespace App.Domain.Employees;

/// <summary>Represents a company employee linked (optionally) 1:1 to a User.</summary>
public sealed class Employee : IAuditableEntity
{
    // --- Key ----------------------------------------------------------------
    public Guid Id { get; private set; } 
    
    // --- Link to User -------------------------------------------------------
    public Users.User? User { get; private set; } = null!;
    public Guid? UserId { get; private set; }
    
    // --- Identity -----------------------------------------------------------
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? PreferredName { get; private set; }
    
    // --- Employment ---------------------------------------------------------
    public EmploymentType? EmploymentType { get; private set; }     // FullTime / PartTime
    public SalaryType? SalaryType { get; private set; }             // Salary / Hourly
    public DateTimeOffset? HireDate { get; private set; }
    public DateTimeOffset? EndDate { get; private set; }            // null => active
    
    // --- Organization -------------------------------------------------------
    public DepartmentType? Department { get; private set; }
    private readonly List<EmployeePosition> _positions = [];
    public IReadOnlyCollection<EmployeePosition> Positions => _positions.AsReadOnly();
    
    // --- Contact / Misc -----------------------------------------------------
    public string? CompanyEmail { get; private set; }               // optional, used to auto-link at registration
    public string? WorkLocation { get; private set; }               // or OfficeId if you add Office
    public string? LicenseNotes { get; private set; }               // e.g., "PLS"
    public string? Notes { get; private set; }
    
    /// <summary>
    /// Recommended role to apply to the new User created for this employee.
    /// Nullable: you can be preapproved without changing default role, or recommend without preapproval.
    /// </summary>
    public Guid? RecommendedRoleId { get; private set; }

    /// <summary>
    /// If true, after email verification a registering user with matching CompanyEmail can be activated immediately.
    /// </summary>
    public bool IsPreapproved { get; private set; }
    
    /* TODO: figure out rates later with Andy */
    
    // --- Auditing ------------------------------------------------------------
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // --- Constructors -------------------------------------------------------
    private Employee() { }
    public Employee(string firstName, string lastName, Guid? userId = null, DateTimeOffset? hireDate = null)
    {
        Id = Guid.CreateVersion7();

        FirstName = Guard.AgainstNullOrWhiteSpace(firstName, nameof(firstName)).ToNormalizedName();
        LastName = Guard.AgainstNullOrWhiteSpace(lastName, nameof(lastName)).ToNormalizedName();

        if (userId is not null)
            UserId = Guard.AgainstDefault(userId.Value, nameof(userId));

        HireDate = hireDate;
        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }
    
    // inside App.Domain.Employees.Employee
    internal Employee(Guid id, string firstName, string lastName, Guid? userId = null, DateTimeOffset? hireDate = null)
        : this(firstName, lastName, userId, hireDate)
    {
        Id = id; // overwrite the v7 with your deterministic seed id
    }
    
    // --- Mutators (guarded, touch) -----------------------------------------
    public void SetName(string first, string last, string? preferred = null)
    {
        EnsureNotDeleted();
        FirstName = Guard.AgainstNullOrWhiteSpace(first, nameof(first)).ToNormalizedName();
        LastName = Guard.AgainstNullOrWhiteSpace(last, nameof(last)).ToNormalizedName();
        PreferredName = string.IsNullOrWhiteSpace(preferred) ? null : preferred.ToNormalizedName();
        Touch();
    }

    public void SetPreferredName(string? preferred)
    {
        EnsureNotDeleted();
        PreferredName = string.IsNullOrWhiteSpace(preferred) ? null : preferred.ToNormalizedName();
        Touch();
    }
    
    public EmployeePosition AddPosition(Guid positionId)
    {
        EnsureNotDeleted();
        var validPositionId = Guard.AgainstDefault(positionId, nameof(positionId));

        // prevent duplicate (EmployeeId, PositionId)
        var existing = _positions.FirstOrDefault(p => p.PositionId == validPositionId);
        if (existing is not null) return existing;

        var ep = new EmployeePosition(Id, validPositionId);
        _positions.Add(ep);
        Touch();
        return ep;
    }

    public void RemovePosition(Guid positionId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(positionId, nameof(positionId));
        var ep = _positions.FirstOrDefault(x => x.PositionId == valid);
        if (ep is null) return;

        _positions.Remove(ep);
        Touch();
    }

    public void LinkUser(Guid userId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(userId, nameof(userId));
        if (UserId == valid) return;
        UserId = valid;
        Touch();
    }

    public void UnlinkUser()
    {
        EnsureNotDeleted();
        if (UserId is null) return;
        UserId = null;
        Touch();
    }
    
    public void RecommendRole(Guid? roleId)
    {
        EnsureNotDeleted();
        if (roleId is { } r) Guard.AgainstDefault(r, nameof(roleId));
        RecommendedRoleId = roleId;
        Touch();
    }

    public void SetPreapproved(bool isPreapproved)
    {
        EnsureNotDeleted();
        if (isPreapproved && string.IsNullOrWhiteSpace(CompanyEmail))
            throw new InvalidOperationException("Cannot preapprove an employee without a company email.");
        IsPreapproved = isPreapproved;
        Touch();
    }

    public void SetEmployment(EmploymentType? type, SalaryType? pay)
    {
        EnsureNotDeleted();
        (EmploymentType, SalaryType) = (type, pay);
        Touch();
    }

    public void SetDates(DateTimeOffset? hire, DateTimeOffset? end)
    {
        EnsureNotDeleted();
        (HireDate, EndDate) = (hire, end);
        Touch();
    }

    public void SetDepartment(DepartmentType? dept)
    {
        EnsureNotDeleted();
        Department = dept;
        Touch();
    }

    public void SetCompanyEmail(string? email)
    {
        EnsureNotDeleted();

        var normalized = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
        CompanyEmail = normalized;

        // If removing email, preapproval must be dropped for safety.
        if (normalized is null && IsPreapproved)
        {
            IsPreapproved = false;
        }

        Touch();
    }

    public void SetWorkLocation(string? location)
    {
        EnsureNotDeleted();
        WorkLocation = location.ToNormalizedAddressLine();
        Touch();
    }

    public void SetLicenseNotes(string? notes)
    {
        EnsureNotDeleted();
        LicenseNotes = notes.ToNormalizedNote();
        Touch();
    }

    public void SetNotes(string? notes)
    {
        EnsureNotDeleted();
        Notes = notes.ToNormalizedNote();
        Touch();
    }

    // --- Lifecycle ----------------------------------------------------------
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

    // --- Helpers ------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted employee.");
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}