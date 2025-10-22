using App.Application.Abstractions;
using App.Domain.Common;
using App.Domain.Users.Events;
using App.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Background;

public sealed class OutboxProcessorWorker(
    IServiceProvider services,
    ILogger<OutboxProcessorWorker> logger) : BackgroundService
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
                var messages = await db.OutboxMessages
                    .Where(m => m.ProcessedAtUtc == null)
                    .OrderBy(m => m.OccurredAtUtc)
                    .Take(10)
                    .ToListAsync(ct);

                if (messages.Count == 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), ct);
                    continue;
                }

                // Step 2: Process each message
                foreach (var msg in messages)
                {
                    try
                    {
                        await HandleMessageAsync(msg, scope.ServiceProvider, ct);
                        msg.MarkProcessed();
                    }
                    catch (Exception ex)
                    {
                        msg.IncrementRetry();
                        logger.LogError(ex, "Failed processing outbox message {Id}", msg.Id);
                    }
                }

                await db.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox processor loop failed");
                await Task.Delay(TimeSpan.FromSeconds(30), ct);
            }
        }

        logger.LogInformation("Outbox processor stopped.");
    }
    
    private static async Task HandleAsync<T>(
        OutboxMessage msg,
        IServiceProvider provider,
        Func<T, IServiceProvider, Task> handler)
    {
        var evt = System.Text.Json.JsonSerializer.Deserialize<T>(msg.Payload);
        if (evt is not null)
            await handler(evt, provider);
    }

    private static async Task HandleMessageAsync(
        OutboxMessage message,
        IServiceProvider provider,
        CancellationToken ct)
    {
        switch (message.Type)
        {
            case "App.Domain.Users.Events.UserRegistered":
                await HandleAsync<UserRegistered>(message, provider,
                    async (userRegistered, sp) =>
                    {
                        var emailSender = sp.GetRequiredService<IEmailSender>();
                        await emailSender.SendWelcomeEmailAsync(userRegistered.Email, ct);
                    });
                break;

            default:
                var logger = provider.GetRequiredService<ILogger<OutboxProcessorWorker>>();
                logger.LogWarning("No handler configured for outbox message type {Type}", message.Type);
                break;
        }
    }
}