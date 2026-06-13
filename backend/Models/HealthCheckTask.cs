namespace UrlMonitor.Api.Models;

/// <summary>Channel message: one pending ping for a monitored URL within a run (job).</summary>
public record HealthCheckTask(Guid JobId, Guid UrlId, string TargetUrl);
