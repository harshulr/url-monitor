using System.ComponentModel.DataAnnotations;

namespace UrlMonitor.Api.Models;

/// <summary>A single endpoint the system monitors.</summary>
public class MonitoredUrl
{
    /// <summary>Unique identifier for the monitoring target.</summary>
    public Guid Id { get; set; }

    /// <summary>The absolute destination URL endpoint (e.g., https://example.com).</summary>
    [Required]
    public required string Url { get; set; }

    /// <summary>Friendly display name shown in the UI.</summary>
    [Required]
    public required string Name { get; set; }

    /// <summary>Toggle background checks without deleting historical records.</summary>
    public bool IsActive { get; set; }

    /// <summary>Timestamp indicating record insertion (UTC).</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Navigation: all historical results recorded for this target.</summary>
    public ICollection<HealthCheckResult> Results { get; set; } = new List<HealthCheckResult>();
}
