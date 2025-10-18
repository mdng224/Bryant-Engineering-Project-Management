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
        Name = name.ToNormalizedName();
        Code = string.IsNullOrWhiteSpace(code) ? null : code.ToNormalizedName();
        RequiresLicense = requiresLicense;
    }
    
    // --- Mutators ------------------------------------------------------
    
    public void Rename(string name)
    {
        Name = name.ToNormalizedName();
    }

    public void SetCode(string? code)
    {
        Code = string.IsNullOrWhiteSpace(code) ? null : code.ToNormalizedName();
    }

    public void RequireLicense(bool requires) => RequiresLicense = requires;
}