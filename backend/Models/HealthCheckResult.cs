namespace UrlMonitor.Api.Models;

/// <summary>The outcome of a single check against a monitored URL (time-series history row).</summary>
public class HealthCheckResult
{
    /// <summary>Unique result identifier.</summary>
    public Guid Id { get; set; }

    /// <summary>FK to <see cref="MonitoredUrl.Id"/>. Used to query a single service's history.</summary>
    public Guid MonitoredUrlId { get; set; }

    /// <summary>The exact point-in-time (UTC) the network call completed.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// HTTP status code returned by the server. Null when no response was produced
    /// (DNS failure, timeout, connection refused).
    /// </summary>
    public int? StatusCode { get; set; }

    /// <summary>Round-trip latency in milliseconds, measured with a Stopwatch.</summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>True only when the server responded with a 2xx status.</summary>
    public bool IsSuccess { get; set; }

    /// <summary>Raw exception message when the request failed without a response.</summary>
    public string? ErrorMessage { get; set; }

    /// <summary>Navigation: the monitored target this result belongs to.</summary>
    public MonitoredUrl? MonitoredUrl { get; set; }
}
