namespace App.Application.Abstractions.Persistence.Writers;

public interface IEmailVerificationWriter
{
    /// <summary>Creates a verification row and returns the RAW token (for email link).</summary>
    string Add(Guid userId);
}