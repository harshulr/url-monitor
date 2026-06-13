namespace UrlMonitor.Api.Models;

/// <summary>Channel message: one pending ping for a monitored URL.</summary>
public record HealthCheckTask(Guid UrlId, string TargetUrl);
