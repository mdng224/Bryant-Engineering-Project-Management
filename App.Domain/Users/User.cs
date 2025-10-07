using App.Domain.Common;

namespace App.Domain.Users;

public class User : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // EF needs a parameterless constructor
    private User() { }

    public User(string email, string passwordHash)
    {
        Id = Guid.NewGuid();
        SetEmail(email);
        SetPasswordHash(passwordHash);
        CreatedAtUtc = UpdatedAtUtc;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new ArgumentException("Email required.", nameof(email));
        Email = NormalizeEmail(email);
        Touch();
    }

    public void SetPasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) throw new ArgumentException("Password hash required.", nameof(hash));
        PasswordHash = hash;
        Touch();
    }

    private void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
