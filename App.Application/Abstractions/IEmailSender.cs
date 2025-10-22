namespace App.Application.Abstractions;

public interface IEmailSender
{
    Task SendWelcomeEmailAsync(string toEmail, CancellationToken ct = default);
}