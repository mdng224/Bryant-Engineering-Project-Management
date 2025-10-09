using App.Domain.Common;

namespace App.Domain.Users;

public class User : IAuditableEntity
{
    public Guid Id { get; private set; }
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public ICollection<UserRole> UserRoles { get; } = [];

    // Let the domain set these; EF config maps to timestamptz.
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    // EF needs a parameterless constructor
    private User() { }

    public User(string email, string passwordHash)
    {
        Id = Guid.CreateVersion7();                 // ordered, time-based GUID (requires .NET 8+)
        Email = NormalizeEmail(Guard.AgainstNullOrWhiteSpace(email, nameof(email)));
        PasswordHash = Guard.AgainstNullOrWhiteSpace(passwordHash, nameof(passwordHash));

        var now = DateTimeOffset.UtcNow;            // FIX: set both timestamps to the same 'now'
        CreatedAtUtc = now;
        UpdatedAtUtc = now;
    }

    // Trim only — DO NOT lowercase here if you want to preserve user-entered casing.
    // Case-insensitive uniqueness is enforced by DB via computed column + unique index.
    private static string NormalizeEmail(string email) => email.Trim();
}
