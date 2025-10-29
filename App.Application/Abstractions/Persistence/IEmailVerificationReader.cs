using App.Domain.Auth;

namespace App.Application.Abstractions.Persistence;

public interface IEmailVerificationReader
{
    Task<EmailVerification?> GetByTokenHashAsync(string token, CancellationToken ct = default);
}