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
    
    // TODO: Add more fields per andy's email
    
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
    
    /* TODO: figure out rates later with Andy */
    
    // --- Auditing ------------------------------------------------------------
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    // --- Constructors -------------------------------------------------------
    private Employee() { }
    public Employee(Guid userId, DateTimeOffset? hireDate = null)
    {
        Id = Guid.CreateVersion7();
        UserId = Guard.AgainstDefault(userId, nameof(userId));
        HireDate = hireDate;
        var now = DateTimeOffset.UtcNow;
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }
    
    // --- Mutators (guarded, touch) -----------------------------------------
    public EmployeePosition AddPosition(Guid positionId)
    {
        EnsureNotDeleted();
        var validPositionId = Guard.AgainstDefault(positionId, nameof(positionId));

        // prevent duplicate assignment
        if (_positions.Any(p => p.PositionId == validPositionId))
            return _positions.First(p => p.PositionId == validPositionId);

        var ep = new EmployeePosition(Id, validPositionId);
        _positions.Add(ep);
        Touch();
        return ep;
    }

    public void RemovePosition(Guid employeePositionId)
    {
        EnsureNotDeleted();
        var id = Guard.AgainstDefault(employeePositionId, nameof(employeePositionId));
        var ep = _positions.FirstOrDefault(x => x.Id == id);
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
        CompanyEmail = string.IsNullOrWhiteSpace(email) ? null : email.ToNormalizedEmail();
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