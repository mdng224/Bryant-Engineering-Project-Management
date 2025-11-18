namespace App.Domain.Auth;

public class EmailVerification
{
    // --- Constructors -------------------------------------------------------
    private EmailVerification() { }

    public EmailVerification(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
        Used = false;
    }
    
    public Guid Id = Guid.CreateVersion7();
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = null!;
    public DateTime ExpiresAtUtc { get; private set; }
    public bool Used { get; private set; }

    // --- Mutators -----------------------------------------------------------
    public void MarkUsed() => Used = true;
}