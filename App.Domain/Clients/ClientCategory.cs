namespace App.Domain.Clients;

public sealed class ClientCategory
{
    // --- Constructors ---------------------------------------------------------
    private ClientCategory() { } // EF

    public ClientCategory(Guid id, string name)
    {
        Id = id;
        Name = name;
    }
    
    // --- Properties -----------------------------------------------------------
    public Guid Id                             { get; private set; }
    public string Name                         { get; private set; } = null!;
    
    public ICollection<ClientType> Types { get; private set; } = new List<ClientType>();
}