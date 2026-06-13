namespace UrlMonitor.Api.Models;

/// <summary>The outcome of a single check against a monitored URL (one history row).</summary>
public class HealthCheckResult
{
    public Guid Id { get; set; }

    /// <summary>The run (job) this result belongs to — the trace anchor for a batch of checks.</summary>
    public Guid JobId { get; set; }

    public Guid MonitoredUrlId { get; set; }

    public DateTime Timestamp { get; set; }

    /// <summary>Null when no response was produced (DNS failure, timeout, connection refused).</summary>
    public int? StatusCode { get; set; }

    public long ResponseTimeMs { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public SchedulerJob? Job { get; set; }

    public MonitoredUrl? MonitoredUrl { get; set; }
}
