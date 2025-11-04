using App.Application.Abstractions.Persistence.Readers;
using App.Application.Abstractions.Persistence.Writers;
using App.Domain.Auth;
using App.Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationRepository(AppDbContext db)
    : IEmailVerificationReader, IEmailVerificationWriter
{
    // --- Writer -------------------------------------------------------------
    public string Add(Guid userId)
    {
        var (rawToken, tokenHash) = TokenGenerator.CreateTokenPair();

        var emailVerification = new EmailVerification(
            userId: userId,
            tokenHash: tokenHash,
            expiresAtUtc: DateTime.UtcNow.AddHours(24));

        db.EmailVerifications.Add(emailVerification);
        return rawToken; // send this in the verify link
    }

    // --- Reader -------------------------------------------------------------
    public async Task<EmailVerification?> GetByTokenHashAsync(string token, CancellationToken ct = default)
    {
        var tokenHash = TokenGenerator.Hash(token);

        return await db.EmailVerifications
            .AsNoTracking()
            .FirstOrDefaultAsync(ev =>
                ev.TokenHash == tokenHash &&
                !ev.Used &&
                ev.ExpiresAtUtc > DateTime.UtcNow, ct);
    }
}