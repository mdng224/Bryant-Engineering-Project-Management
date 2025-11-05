using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Employees;

/// <summary>Represents a company employee linked (optionally) 1:1 to a User.</summary>
public sealed class Employee : IAuditableEntity, ISoftDeletable
{
    // --- Key ----------------------------------------------------------------
    public Guid Id { get; private set; } 
    
    // --- Link to User -------------------------------------------------------
    public Users.User? User { get; private set; }
    public Guid? UserId { get; private set; }
    
    // --- Identity -----------------------------------------------------------
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? PreferredName { get; private set; }
    
    // --- Employment --------------------------------------------------------
    public EmploymentType? EmploymentType { get; private set; }   // FullTime / PartTime
    public SalaryType?     SalaryType     { get; private set; }   // Salary / Hourly
    public DateTimeOffset? HireDate       { get; private set; }
    public DateTimeOffset? EndDate        { get; private set; }   // null => active
    
    // --- Organization -------------------------------------------------------
    public DepartmentType? Department { get; private set; }
    private readonly List<EmployeePosition> _positions = [];
    public IReadOnlyCollection<EmployeePosition> Positions => _positions.AsReadOnly();
    

    // --- Contact / Misc ----------------------------------------------------
    public Address?  Address       { get; private set; }
    public string?   CompanyEmail  { get; private set; } // may auto-link at registration
    public string?   WorkLocation  { get; private set; }
    public string?   Notes         { get; private set; }

    /// <summary>Role to apply if/when a User is created for this employee.</summary>
    public Guid? RecommendedRoleId { get; private set; }

    /// <summary>If true, a registering user with matching CompanyEmail can be auto-activated.</summary>
    public bool IsPreapproved      { get; private set; }
    
    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }

    public bool IsDeleted => DeletedAtUtc.HasValue;

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
    }
    
    // inside App.Domain.Employees.Employee
    internal Employee(Guid id, string firstName, string lastName, Guid? userId = null, DateTimeOffset? hireDate = null)
        : this(firstName, lastName, userId, hireDate)
    {
        Id = id; // overwrite the v7 with your deterministic seed id
    }
    
    // --- Mutators -----------------------------------------
    public void SetName(string first, string last, string? preferred = null)
    {
        EnsureNotDeleted();
        FirstName = Guard.AgainstNullOrWhiteSpace(first, nameof(first)).ToNormalizedName();
        LastName = Guard.AgainstNullOrWhiteSpace(last, nameof(last)).ToNormalizedName();
        PreferredName = string.IsNullOrWhiteSpace(preferred) ? null : preferred.ToNormalizedName();
    }

    public void SetPreferredName(string? preferred)
    {
        EnsureNotDeleted();
        PreferredName = string.IsNullOrWhiteSpace(preferred) ? null : preferred.ToNormalizedName();
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

        return ep;
    }

    public void RemovePosition(Guid positionId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(positionId, nameof(positionId));
        var ep = _positions.FirstOrDefault(x => x.PositionId == valid);
        if (ep is null) return;

        _positions.Remove(ep);
    }

    public void LinkUser(Guid userId)
    {
        EnsureNotDeleted();
        var valid = Guard.AgainstDefault(userId, nameof(userId));
        if (UserId == valid) return;
        UserId = valid;
    }

    public void UnlinkUser()
    {
        EnsureNotDeleted();
        if (UserId is null) return;
        UserId = null;
    }
    
    public void RecommendRole(Guid? roleId)
    {
        EnsureNotDeleted();
        if (roleId is { } r) Guard.AgainstDefault(r, nameof(roleId));
        RecommendedRoleId = roleId;
    }

    public void SetAddress(Address? address)
    {
        EnsureNotDeleted();

        // if null — clear the address
        switch (address)
        {
            case null when Address is null:
                return;
            case null when Address is not null:
                Address = null;
        
                return;
        }

        // Normalize input values
        var line1 = address!.Line1.ToNormalizedAddressLine();
        var line2 = address.Line2.ToNormalizedAddressLine();
        var city = address.City.ToNormalizedCity();
        var state = address.State.ToNormalizedState();
        var postalCode = address.PostalCode.ToNormalizedPostal();

        // Construct a new normalized address value object
        var newAddress = new Address(line1, line2, city, state, postalCode);

        // Only update if something actually changed
        if (Address == newAddress)
            return;

        Address = newAddress;
    }
    
    public void SetPreapproved(bool isPreapproved)
    {
        EnsureNotDeleted();
        if (isPreapproved && string.IsNullOrWhiteSpace(CompanyEmail))
            throw new InvalidOperationException("Cannot preapprove an employee without a company email.");
        IsPreapproved = isPreapproved;
    }

    public void SetEmployment(EmploymentType? type, SalaryType? pay)
    {
        EnsureNotDeleted();
        (EmploymentType, SalaryType) = (type, pay);
    }

    public void SetDates(DateTimeOffset? hire, DateTimeOffset? end)
    {
        EnsureNotDeleted();
        (HireDate, EndDate) = (hire, end);
    }

    public void SetDepartment(DepartmentType? dept)
    {
        EnsureNotDeleted();
        Department = dept;
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
    }

    public void SetWorkLocation(string? location)
    {
        EnsureNotDeleted();
        WorkLocation = location.ToNormalizedAddressLine();
    }

    public void SetNotes(string? notes)
    {
        EnsureNotDeleted();
        Notes = notes.ToNormalizedNote();
    }

    // --- Lifecycle ----------------------------------------------------------
    public void SoftDelete()
    {
        if (DeletedAtUtc is not null) return;
        DeletedAtUtc = DateTimeOffset.UtcNow;
        UpdatedAtUtc = DeletedAtUtc.Value;
    }

    public bool Restore()
    {
        if (!IsDeleted)
            return false;
        
        DeletedAtUtc = null;
        DeletedById = null;
        return true;
    }

    // --- Helpers ------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted employee.");
    }
}