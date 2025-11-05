using App.Domain.Common;
using App.Domain.Common.Abstractions;

namespace App.Domain.Employees;

public sealed class Position : IAuditableEntity, ISoftDeletable
{
    // --- Key ----------------------------------------------------------------
    public Guid Id                     { get; private set; }
    
    // --- Data --------------------------------------------------------------
    public string Name                 { get; private set; } = null!;
    public string? Code                { get; private set; }
    public bool RequiresLicense        { get; private set; }
    
    // --- Auditing ----------------------------------------------------------
    public DateTimeOffset CreatedAtUtc  { get; private set; }
    public DateTimeOffset UpdatedAtUtc  { get; private set; }
    public DateTimeOffset? DeletedAtUtc { get; private set; }

    public Guid? CreatedById            { get; private set; }
    public Guid? UpdatedById            { get; private set; }
    public Guid? DeletedById            { get; private set; }
    public bool IsDeleted => DeletedAtUtc.HasValue;
    
    // --- Constructors -------------------------------------------------------
    private Position() { }
    
    public Position(string name, string? code = null, bool requiresLicense = false)
    {
        Id = Guid.CreateVersion7();
        SetCore(name, code, requiresLicense);
    }

    // --- Seeding helper -----------------------------------------------------
    public static Position CreateSeed(Guid id, string name, string? code = null, bool requiresLicense = false)
        => new(name, code, requiresLicense) { Id = id };

    // --- Single intent method ----------------------------------------------
    /// <summary>Updates all fields at once. Returns true if anything changed.</summary>
    public void Update(string name, string? code, bool requiresLicense)
    {
        EnsureNotDeleted();
        SetCore(name, code, requiresLicense);
    }

    public void RestoreAndUpdate(string name, string? code, bool requiresLicense)
    {
        if (IsDeleted)
        {
            DeletedAtUtc = null;
            DeletedById  = null;
        }

        Update(name, code, requiresLicense);
    }
    
    public bool Restore()
    {
        if (!IsDeleted)
            return false;
        
        DeletedAtUtc = null;
        DeletedById = null;
        return true;
    }

    // --- Core logic (single source of truth) --------------------------------
    private void SetCore(string name, string? code, bool requiresLicense)
    {
        Name = Guard.AgainstNullOrWhiteSpace(name, nameof(name)).ToNormalizedName();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.Trim().ToUpperInvariant();
        RequiresLicense = requiresLicense;
    }

    // --- Helpers --------------------------------------------------------------
    private void EnsureNotDeleted()
    {
        if (DeletedAtUtc is not null)
            throw new InvalidOperationException("Cannot mutate a soft-deleted position.");
    }
}