using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Employees;

/// <summary>Represents a company employee linked (optionally) 1:1 to a User.</summary>
public sealed class Employee : IAuditableEntity, ISoftDeletable
{
    // --- Constructors -------------------------------------------------------
    private Employee() { }
    public Employee(
        string firstName,
        string lastName,
        string? preferredName,
        Guid? userId,
        EmploymentType? employmentType,
        SalaryType? salaryType,
        DepartmentType? department,
        DateTimeOffset? hireDate,
        string? companyEmail,
        string? workLocation,
        string? notes,
        string? line1,
        string? line2,
        string? city,
        string? state,
        string? postalCode,
        Guid? recommendedRoleId,
        bool isPreapproved)
    {
        Id = Guid.CreateVersion7();
        SetName(firstName, lastName, preferredName);
        if (userId is { } id)
            UserId = Guard.AgainstDefault(id, nameof(userId));

        HireDate = hireDate;

        SetEmployment(employmentType, salaryType);
        SetDepartment(department);
        SetCompanyEmail(companyEmail);
        SetWorkLocation(workLocation);
        SetNotes(notes);
        RecommendRole(recommendedRoleId);

        var address = CreateAddressOrNull(
            line1,
            line2,
            city,
            state,
            postalCode
        );
        SetAddress(address);

        // Must come AFTER SetCompanyEmail (it checks for company email)
        SetPreapproved(isPreapproved);
    }
    
    public Guid Id { get; private set; } 
    public Users.User? User { get; private set; }
    public Guid? UserId { get; private set; }
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? PreferredName { get; private set; }
    public EmploymentType? EmploymentType { get; private set; }   // FullTime / PartTime
    public SalaryType?     SalaryType     { get; private set; }   // Salary / Hourly
    public DateTimeOffset? HireDate       { get; private set; }
    public DateTimeOffset? EndDate        { get; private set; }   // null => active
    public DepartmentType? Department { get; private set; }
    private readonly List<EmployeePosition> _positions = [];
    public IReadOnlyCollection<EmployeePosition> Positions => _positions.AsReadOnly();
    public Address?  Address       { get; private set; }
    public string?   CompanyEmail  { get; private set; } // may auto-link at registration
    public string?   WorkLocation  { get; private set; }
    public string?   Notes         { get; private set; }
    public Guid? RecommendedRoleId { get; private set; }
    public bool IsPreapproved      { get; private set; }
    
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }
    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;

    internal static Employee Seed(
        Guid id,
        string last,
        string legalFirst,
        string? nickname,
        DepartmentType? departmentType,
        IEnumerable<Guid>? positionIds,
        EmploymentType? employmentType,
        SalaryType? salaryType,
        string? companyEmail,
        Guid? recommendedRoleId = null,
        bool isPreapproved = false,
        bool isFormerEmployee = false)
    {
        var employee = new Employee(
            firstName:         legalFirst,
            lastName:          last,
            preferredName:     nickname,
            userId:            null,
            employmentType:    employmentType,
            salaryType:        salaryType,
            department:        departmentType,
            hireDate:          null,
            companyEmail:      companyEmail,
            workLocation:      null,
            notes:             null,
            line1:             null,
            line2:             null,
            city:              null,
            state:             null,
            postalCode:        null,
            recommendedRoleId: recommendedRoleId,
            isPreapproved:     isPreapproved
        )
        {
            Id = id
        };

        if (positionIds is not null)
        {
            foreach (var pid in positionIds)
                employee.AddPosition(pid);
        }

        if (isFormerEmployee)
            employee.SoftDelete();

        return employee;
    }
    
    // --- Mutators -----------------------------------------
    private void SetName(string first, string last, string? preferred)
    {
        EnsureNotDeleted();
        FirstName = Guard.AgainstNullOrWhiteSpace(first, nameof(first)).ToNormalizedName();
        LastName = Guard.AgainstNullOrWhiteSpace(last, nameof(last)).ToNormalizedName();
        PreferredName = preferred?.ToNormalizedName();
    }

    private void AddPosition(Guid positionId)
    {
        EnsureNotDeleted();
        var validPositionId = Guard.AgainstDefault(positionId, nameof(positionId));

        // prevent duplicate (EmployeeId, PositionId)
        var existing = _positions.FirstOrDefault(p => p.PositionId == validPositionId);
        if (existing is not null) return;

        var ep = new EmployeePosition(Id, validPositionId);
        _positions.Add(ep);
    }
    
    private void RecommendRole(Guid? roleId)
    {
        EnsureNotDeleted();
        if (roleId is { } r) Guard.AgainstDefault(r, nameof(roleId));
        RecommendedRoleId = roleId;
    }

    /// <summary>
    /// Sets the employee's address with an all-or-nothing rule:
    /// either null (no address) or a fully specified address (line1, city, state, postalCode).
    /// </summary>
    private void SetAddress(Address? address)
    {
        EnsureNotDeleted();
        ApplyAddress(address);
    }
    
    private void SetPreapproved(bool isPreapproved)
    {
        EnsureNotDeleted();
        if (isPreapproved && string.IsNullOrWhiteSpace(CompanyEmail))
            throw new InvalidOperationException("Cannot preapprove an employee without a company email.");
        IsPreapproved = isPreapproved;
    }

    private void SetEmployment(EmploymentType? employmentType, SalaryType? salaryType)
    {
        EnsureNotDeleted();
        (EmploymentType, SalaryType) = (employmentType, salaryType);
    }

    private void SetDepartment(DepartmentType? dept)
    {
        EnsureNotDeleted();
        Department = dept;
    }

    private void SetCompanyEmail(string? email)
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

    private void SetWorkLocation(string? location)
    {
        EnsureNotDeleted();
        WorkLocation = string.IsNullOrWhiteSpace(location)
            ? null
            : location.ToNormalizedAddressLine();
    }

    private void SetNotes(string? notes)
    {
        EnsureNotDeleted();
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.ToNormalizedNote();
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
    
    private void ApplyAddress(Address? address)
    {
        if (address is null)
        {
            if (Address is null) return;
            Address = null;
            return;
        }

        // Re-apply normalization + all-or-nothing semantics using raw strings.
        var newAddress = CreateAddressOrNull(
            address.Line1,
            address.Line2,
            address.City,
            address.State,
            address.PostalCode
        );

        if (Address == newAddress) return;

        Address = newAddress;
    }
    
    private static Address? CreateAddressOrNull(
        string? line1,
        string? line2,
        string? city,
        string? state,
        string? postalCode)
    {
        // If everything is blank => no address
        if (string.IsNullOrWhiteSpace(line1)
            && string.IsNullOrWhiteSpace(line2)
            && string.IsNullOrWhiteSpace(city)
            && string.IsNullOrWhiteSpace(state)
            && string.IsNullOrWhiteSpace(postalCode))
        {
            return null;
        }

        // All-or-nothing: if any address info is provided,
        // all required fields must be present.
        if (string.IsNullOrWhiteSpace(line1)
            || string.IsNullOrWhiteSpace(city)
            || string.IsNullOrWhiteSpace(state)
            || string.IsNullOrWhiteSpace(postalCode))
        {
            throw new InvalidOperationException(
                "Address must be fully provided (line1, city, state, postal code) or not provided at all."
            );
        }
        
        if (state.Trim().Length != 2)
            throw new InvalidOperationException("State must be a 2-letter code (e.g., KY).");
        
        // Normalize after we've verified the required fields
        var normalizedLine1      = line1.ToNormalizedAddressLine();
        var normalizedLine2      = line2.ToNormalizedAddressLine();
        var normalizedCity       = city.ToNormalizedCity();
        var normalizedState      = state.ToNormalizedState();
        var normalizedPostalCode = postalCode.ToNormalizedPostal();

        return new Address(
            normalizedLine1!,
            normalizedLine2,
            normalizedCity!,
            normalizedState!,
            normalizedPostalCode!
        );
    }
}