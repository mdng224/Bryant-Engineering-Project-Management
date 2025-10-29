using App.Domain.Auth;

namespace App.Application.Abstractions.Persistence;

public interface IEmailVerificationWriter
{
    /// <summary>Creates a verification row and returns the RAW token (for email link).</summary>
    Task<string> CreateAsync(Guid userId, CancellationToken ct = default);
    Task MarkUsedAsync(Guid verificationId, CancellationToken ct = default);
    Task UpdateAsync(EmailVerification emailVerification, CancellationToken ct = default);
}