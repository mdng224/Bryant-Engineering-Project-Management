using App.Application.Abstractions;
using App.Domain.Auth;
using App.Infrastructure.Auth;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationRepository(AppDbContext db)
    : IEmailVerificationReader, IEmailVerificationWriter
{
    // --- Writer -------------------------------------------------------------
    public async Task<string> CreateAsync(Guid userId, CancellationToken ct = default)
    {
        var (rawToken, tokenHash) = TokenGenerator.CreateTokenPair();

        var emailVerification = new EmailVerification(
            userId: userId,
            tokenHash: tokenHash,
            expiresAtUtc: DateTime.UtcNow.AddHours(24));

        await db.EmailVerifications.AddAsync(emailVerification, ct);
        await db.SaveChangesAsync(ct);
        
        return rawToken; // send this in the verify link
    }

    public async Task MarkUsedAsync(Guid verificationId, CancellationToken ct = default)
    {
        var emailVerification = await db.EmailVerifications.FindAsync([verificationId], ct);
        if (emailVerification is null)
            return;

        emailVerification.MarkUsed();
        await UpdateAsync(emailVerification, ct);
    }

    public async Task UpdateAsync(EmailVerification emailVerification, CancellationToken ct = default)
    {
        db.EmailVerifications.Update(emailVerification);
        await db.SaveChangesAsync(ct);
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