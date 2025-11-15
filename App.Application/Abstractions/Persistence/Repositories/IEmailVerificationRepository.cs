using App.Domain.Auth;

namespace App.Application.Abstractions.Persistence.Repositories;

public interface IEmailVerificationRepository
{
    /// <summary>Creates a verification row and returns the RAW token (for email link).</summary>
    string Add(Guid userId);
    Task<EmailVerification?> GetForUpdateByTokenHashAsync(string token, CancellationToken ct = default);
}