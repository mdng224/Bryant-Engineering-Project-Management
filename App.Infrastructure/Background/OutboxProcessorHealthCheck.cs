using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace App.Infrastructure.Background;

public sealed class OutboxProcessorHealthCheck(
    ILogger<OutboxProcessorHealthCheck> logger) : IHealthCheck
{
    private static DateTimeOffset _lastProcessedUtc = DateTimeOffset.MinValue;
    public static void UpdateLastProcessed() => _lastProcessedUtc = DateTimeOffset.UtcNow;

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct = default)
    {
        var age = DateTimeOffset.UtcNow - _lastProcessedUtc;
        var healthy = age < TimeSpan.FromMinutes(5); // no work in >5min = unhealthy

        if (healthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy(
                $"Last outbox message processed {age.TotalSeconds:F0}s ago."));
        }

        logger.LogWarning("Outbox processor health degraded — last processed {Age} ago", age);
        
        return Task.FromResult(HealthCheckResult.Degraded(
            $"No outbox message processed for {age.TotalMinutes:F1} minutes."));
    }
}