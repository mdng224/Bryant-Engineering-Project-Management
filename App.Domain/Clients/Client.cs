namespace App.Domain.Clients;

public sealed class Client
{
    public Guid UserId { get; private set; }                 // PK & FK to Users
    public string? CompanyName { get; private set; }

    // nav
    public Users.User User { get; private set; } = null!;

    private Client() { }
    public Client(Guid userId, string? companyName = null)
        => (UserId, CompanyName) = (userId, companyName);
}