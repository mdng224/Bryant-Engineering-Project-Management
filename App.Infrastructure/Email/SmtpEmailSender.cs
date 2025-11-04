using App.Application.Abstractions.Messaging;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace App.Infrastructure.Email;

public sealed partial class SmtpEmailSender(
    IOptions<EmailSettings> options,
    ILogger<SmtpEmailSender> logger) : IEmailSender
{
    private readonly EmailSettings _settings = options.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        // --- Basic validation (fail fast) -----------------------------------
        if (string.IsNullOrWhiteSpace(_settings.Host))
            throw new InvalidOperationException("EmailSettings.Host is required.");
        if (_settings.Port <= 0)
            throw new InvalidOperationException("EmailSettings.Port must be > 0.");
        if (string.IsNullOrWhiteSpace(_settings.From))
            throw new InvalidOperationException("EmailSettings.From is required.");
        
        // --- Build message ---------------------------------------------------
        var mimeMessage = new MimeMessage();
        mimeMessage.From.Add(new MailboxAddress(_settings.FromName ?? string.Empty, _settings.From));
        mimeMessage.To.Add(MailboxAddress.Parse(to));
        mimeMessage.Subject = subject;

        // Provide a simple text alternative (helps deliverability)
        var builder = new BodyBuilder
        {
            HtmlBody = htmlBody,
            TextBody = StripTagsForAltText(htmlBody)
        };
        mimeMessage.Body = builder.ToMessageBody();


        try
        {
            using var smtpClient = new SmtpClient();

            // Gmail on 587 expects STARTTLS. For local dev servers (MailDev/MailHog), use None.
            var secure = _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            logger.LogInformation(
                "SMTP connecting to {Host}:{Port} (UseSsl={UseSsl}) auth={Auth}",
                _settings.Host, _settings.Port, _settings.UseSsl,
                string.IsNullOrWhiteSpace(_settings.Username) ? "none" : "username-present");
            logger.LogInformation("SMTP auth user = {User}", 
                string.IsNullOrWhiteSpace(_settings.Username) ? "(none)" : _settings.Username);

            await smtpClient.ConnectAsync(_settings.Host, _settings.Port, secure, ct);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
                await smtpClient.AuthenticateAsync(_settings.Username, _settings.Password, ct);

            await smtpClient.SendAsync(mimeMessage, ct);
            await smtpClient.DisconnectAsync(true, ct);

            logger.LogInformation("✅ Email sent to {Email}", to);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed sending email to {Email}", to);
            throw;
        }
    }
    
    private static string StripTagsForAltText(string html)
    {
        if (string.IsNullOrWhiteSpace(html))
            return string.Empty;

        var text = HtmlTagRegex().Replace(html, " ");
        
        return MultiSpaceRegex().Replace(text, " ").Trim();
    }

    [System.Text.RegularExpressions.GeneratedRegex("<[^>]+>")]
    private static partial System.Text.RegularExpressions.Regex HtmlTagRegex();
    
    [System.Text.RegularExpressions.GeneratedRegex(@"\s{2,}")]
    private static partial System.Text.RegularExpressions.Regex MultiSpaceRegex();
}