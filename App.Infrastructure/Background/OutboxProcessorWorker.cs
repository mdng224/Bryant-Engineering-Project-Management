using App.Application.Abstractions.Messaging;
using App.Application.Abstractions.Persistence;
using App.Application.Abstractions.Persistence.Writers;
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
                var batchDb  = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Step 1: Fetch unprocessed messages
                var items = await batchDb .OutboxMessages
                    .Where(om => om.ProcessedAtUtc == null)
                    .OrderBy(om => om.OccurredAtUtc)
                    .Take(10)
                    .Select(om => om.Id)
                    .ToListAsync(ct);

                if (items.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    continue;
                }

                // Step 2: Process each message
                foreach (var id in items)
                {
                    using var msgScope = services.CreateScope();
                    var sp   = msgScope.ServiceProvider;
                    var db   = sp.GetRequiredService<AppDbContext>();
                    var uow  = sp.GetRequiredService<IUnitOfWork>();
                    
                    var msg = await db.OutboxMessages.FindAsync([id], ct);
                    if (msg is null || msg.ProcessedAtUtc != null) continue;
                    
                    try
                    {
                        if (msg.RetryCount > 10)
                        {
                            logger.LogWarning("🚫 Skipping outbox message {Id} after {Retries} retries", msg.Id, msg.RetryCount);
                            continue;
                        }
                           
                        // 1) Handle event -> may create verification token/etc. (track only)
                        var send = HandleMessageWithSameScope(msg, sp, config);
                        await uow.SaveChangesAsync(ct);

                        if (send is not null)
                        {
                            var emailSender = sp.GetRequiredService<IEmailSender>();
                            await emailSender.SendAsync(send.To, send.Subject, send.HtmlBody, ct);
                        }

                        // 4) Mark processed and commit
                        msg.MarkProcessed();
                        await uow.SaveChangesAsync(ct);
                        logger.LogInformation("✅ Processed outbox message {Id} ({Type})", msg.Id, msg.Type);
                    }
                    catch (Exception ex)
                    {
                        msg.IncrementRetry();
                        logger.LogError(ex, "❌ Failed processing outbox message {Id}", msg.Id);
                        await uow.SaveChangesAsync(ct); 
                        // optional: exponential backoff (e.g., 2^retries seconds, capped)
                        var delaySeconds = Math.Min(Math.Pow(2, msg.RetryCount), 300); // max 5 min
                        await Task.Delay(TimeSpan.FromSeconds(delaySeconds), ct);
                    }
                }

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
    
    private static SendInstruction? HandleMessageWithSameScope(
        OutboxMessage outboxMessage,
        IServiceProvider sp,
        IConfiguration config)
    {
        switch (outboxMessage.Type)
        {
            case "App.Domain.Users.Events.UserRegistered":
            {
                var ur = System.Text.Json.JsonSerializer.Deserialize<UserRegistered>(outboxMessage.Payload);
                if (ur is null) return null;

                var emailVerificationWriter = sp.GetRequiredService<IEmailVerificationWriter>();
                var rawToken = emailVerificationWriter.Add(ur.UserId);

                var baseUrl = config["Auth:Verification:BaseUrl"]
                    ?? throw new InvalidOperationException("Missing Auth:Verification:BaseUrl in configuration.");
                var verifyUrl = $"{baseUrl}?token={Uri.EscapeDataString(rawToken)}";

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
                        <p style="font-size:12px;color:#6b7280">This link will expire in 24 hours.</p>
                    </div>
                """;

                return new SendInstruction(ur.Email, "Verify your account", htmlBody);
            }

            default:
                var logger = sp.GetRequiredService<ILogger<OutboxProcessorWorker>>();
                logger.LogWarning("⚠️ No handler configured for outbox message type {Type}", outboxMessage.Type);
                return null;
        }
    }
}
