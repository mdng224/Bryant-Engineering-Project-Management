using App.Domain.Common;

namespace App.Domain.Employees;

public sealed class Position
{
    // --- Key ----------------------------------------------------------------
    public Guid Id { get; private set; }
    
    // --- Data --------------------------------------------------------------
    public string Name { get; private set; } = null!;
    public string? Code { get; private set; }
    public bool RequiresLicense { get; private set; }
    
    // --- Constructors -------------------------------------------------------
    private Position() { }
    
    public Position(string name, string? code = null, bool requiresLicense = false)
    {
        Id = Guid.CreateVersion7();
        SetCore(name, code, requiresLicense);
    }

    // --- Seeding helper -----------------------------------------------------
    public static Position CreateSeed(Guid id, string name, string? code = null, bool requiresLicense = false)
    {
        var position = new Position(name, code, requiresLicense)
        {
            Id = id
        };
        return position;
    }

    // --- Single intent method ----------------------------------------------
    /// <summary>Updates all fields at once. Returns true if anything changed.</summary>
    public bool Update(string name, string? code, bool requiresLicense)
        => SetCore(name, code, requiresLicense);

    // --- Core logic (single source of truth) --------------------------------
    private bool SetCore(string name, string? code, bool requiresLicense)
    {
        var newName = Guard.AgainstNullOrWhiteSpace(name, nameof(name)).ToNormalizedName();
        // If you have a dedicated normalizer for codes, use it here:
        // var newCode = string.IsNullOrWhiteSpace(code) ? null : code.ToNormalizedCode();
        var newCode = string.IsNullOrWhiteSpace(code) ? null : code.ToNormalizedName();

        var changed = false;

        if (newName != Name) { Name = newName; changed = true; }
        if (newCode != Code) { Code = newCode; changed = true; }
        if (requiresLicense != RequiresLicense) { RequiresLicense = requiresLicense; changed = true; }

        return changed;
    }
}