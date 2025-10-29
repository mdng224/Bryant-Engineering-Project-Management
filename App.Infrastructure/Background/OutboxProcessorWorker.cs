using App.Application.Abstractions;
using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Domain.Common;
using App.Domain.Users.Events;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Background;

public sealed class OutboxProcessorWorker(
    IServiceProvider services,
    ILogger<OutboxProcessorWorker> logger,
    IConfiguration config) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        logger.LogInformation("Outbox processor started.");
        
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Step 1: Fetch unprocessed messages
                var outboxMessages = await db.OutboxMessages
                    .Where(m => m.ProcessedAtUtc == null)
                    .OrderBy(m => m.OccurredAtUtc)
                    .Take(10)
                    .ToListAsync(ct);

                if (outboxMessages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    continue;
                }

                // Step 2: Process each message
                foreach (var msg in outboxMessages)
                {
                    try
                    {
                        if (msg.RetryCount > 10)
                        {
                            logger.LogWarning("🚫 Skipping message {Id} after {Retries} retries", msg.Id, msg.RetryCount);
                            continue;
                        }
                        await HandleMessageAsync(msg, scope.ServiceProvider, config, ct);
                        msg.MarkProcessed();
                    }
                    catch (Exception ex)
                    {
                        msg.IncrementRetry();
                        logger.LogError(ex, "❌ Failed processing outbox message {Id}", msg.Id);

                        // optional: exponential backoff (e.g., 2^retries seconds, capped)
                        var delaySeconds = Math.Min(Math.Pow(2, msg.RetryCount), 300); // max 5 min
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
                    }
                }

                await db.SaveChangesAsync(ct);
                OutboxProcessorHealthCheck.UpdateLastProcessed(); // ✅ heartbeat
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor loop failed");
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }

        logger.LogInformation("Outbox processor stopped.");
    }
    
    /// <summary>Generic handler deserialization helper</summary>
    private static async Task HandleAsync<T>(
        OutboxMessage outboxMessage,
        IServiceProvider provider,
        Func<T, IServiceProvider, Task> handler)
    {
        var evt = System.Text.Json.JsonSerializer.Deserialize<T>(outboxMessage.Payload);
        if (evt is not null)
            await handler(evt, provider);
    }

    private static async Task HandleMessageAsync(
        OutboxMessage outboxMessage,
        IServiceProvider provider,
        IConfiguration config,
        CancellationToken ct)
    {
        switch (outboxMessage.Type)
        {
            case "App.Domain.Users.Events.UserRegistered":
                await HandleAsync<UserRegistered>(outboxMessage, provider,
                    (ur, sp) => HandleUserRegisteredAsync(ur, sp, config, ct));
                break;

            default:
                var logger = provider.GetRequiredService<ILogger<OutboxProcessorWorker>>();
                logger.LogWarning("⚠️ No handler configured for outbox message type {Type}", outboxMessage.Type);
                break;
        }
    }
    
    /// <summary>Helper: handle specific event (UserRegistered)</summary>
    private static async Task HandleUserRegisteredAsync(
        UserRegistered ur,
        IServiceProvider sp,
        IConfiguration config,
        CancellationToken ct)
    {
        var logger = sp.GetRequiredService<ILogger<OutboxProcessorWorker>>();
        var emailSender = sp.GetRequiredService<IEmailSender>();
        var emailVerificationWriter = sp.GetRequiredService<IEmailVerificationWriter>();

        // 1️⃣ Create verification token
        string token;
        try
        {
            token = await emailVerificationWriter.CreateAsync(ur.UserId, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Failed to create verification token for {UserId}", ur.UserId);
            throw; // fail fast
        }

        // 2️⃣ Build verification URL
        var baseUrl = config["Auth:Verification:BaseUrl"]
                      ?? throw new InvalidOperationException("Missing Auth:Verification:BaseUrl in configuration.");
        var verifyUrl = $"{baseUrl}?token={Uri.EscapeDataString(token)}";

        // 3️⃣ Build HTML email body
        var htmlBody = $"""
                        <div style="font-family:Segoe UI,Roboto,sans-serif;max-width:600px;margin:auto;padding:16px;background:#f9fafb;border-radius:12px">
                            <h2 style="color:#111827">Welcome 👋</h2>
                            <p>Thanks for registering your account.</p>
                            <p>Please verify your email address by clicking below:</p>
                            <p style="margin:24px 0">
                                <a href="{verifyUrl}" 
                                   style="display:inline-block;background:#2563eb;color:white;padding:10px 18px;
                                          border-radius:6px;text-decoration:none;font-weight:500">
                                   Verify Email
                                </a>
                            </p>
                            <p>If the button doesn't work, copy and paste this link:</p>
                            <p style="word-break:break-all"><a href="{verifyUrl}">{verifyUrl}</a></p>
                            <hr style="border:none;border-top:1px solid #e5e7eb;margin:24px 0"/>
                            <p style="font-size:12px;color:#6b7280">
                                This link will expire in 24 hours.
                            </p>
                        </div>
                    """;

        // 4️⃣ Send email
        try
        {
            await emailSender.SendAsync(
                to: ur.Email,
                subject: "Verify your account",
                htmlBody: htmlBody,
                ct: ct);

            logger.LogInformation("✅ Sent verification email to {Email}", ur.Email);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "❌ Email send failed for {Email}", ur.Email);
            // optional: mark the verification record as unused
            throw; // ensures retry but prevents extra tokens
        }

        logger.LogInformation("✅ Sent verification email to {userEmail}", ur.Email);
    }
}
