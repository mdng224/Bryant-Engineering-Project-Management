using App.Application.Abstractions.Persistence.Repositories;
using App.Domain.Auth;
using App.Infrastructure.Auth;

namespace App.Infrastructure.Persistence.Repositories;

public sealed class EmailVerificationRepository(AppDbContext db) : IEmailVerificationRepository
{
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
}