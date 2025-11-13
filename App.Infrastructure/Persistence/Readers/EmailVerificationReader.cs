using App.Application.Abstractions.Persistence.Readers;
using App.Domain.Auth;
using App.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Readers;

public sealed class EmailVerificationReader(AppDbContext db) : IEmailVerificationReader
{
    public async Task<EmailVerification?> GetByTokenHashAsync(string token, CancellationToken ct = default)
    {
        var tokenHash = TokenGenerator.Hash(token);

        return await db.ReadSet<EmailVerification>()
            .FirstOrDefaultAsync(ev =>
                ev.TokenHash == tokenHash
                && !ev.Used
                && ev.ExpiresAtUtc > DateTime.UtcNow, ct);
    }
}