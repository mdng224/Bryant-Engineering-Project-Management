using App.Application.Abstractions;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Email;

public sealed class ConsoleEmailSender(ILogger<ConsoleEmailSender> logger) : IEmailSender
{
    public Task SendWelcomeEmailAsync(string toEmail, CancellationToken ct = default)
    {
        logger.LogInformation("📧 [EmailSender] Sending welcome email to {Email}", toEmail);

        // Simulate some delay to mimic network I/O
        return Task.Delay(500, ct);
    }
}