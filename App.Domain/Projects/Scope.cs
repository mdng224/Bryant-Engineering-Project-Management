namespace App.Domain.Projects;

public sealed class Scope
{
    // --- Constructors --------------------------------------------------------
    private Scope() { } // EF

    public Scope(Guid id, string name, string? description)
    {
        Id = id;
        Name = string.IsNullOrWhiteSpace(name)
            ? throw new ArgumentException("Scope name is required.", nameof(name))
            : name.Trim();
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
    }
    
    // --- Properties -----------------------------------------------------------
    public Guid Id             { get; private set; }
    public string Name          { get; private set; } = null!;
    public string? Description  { get; private set; }
}