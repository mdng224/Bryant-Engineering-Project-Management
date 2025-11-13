namespace App.Domain.Clients;

public sealed class ClientType
{
    // --- Constructors --------------------------------------------------------
    private ClientType() { } // EF

    public ClientType(Guid id, string name, string description, Guid categoryId)
    {
        Id = id;
        Name = name;
        Description = description;
        CategoryId = categoryId;
    }

    // --- Properties -----------------------------------------------------------
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string Description                  { get; private set; } = null!;
    
    // required FK to Category
    public Guid CategoryId { get; private set; }
    public ClientCategory Category { get; private set; } = null!;
}