using System.ComponentModel.DataAnnotations;

namespace UrlMonitor.Api.Models;

/// <summary>An endpoint the system monitors.</summary>
public class MonitoredUrl
{
    public Guid Id { get; set; }

    [Required]
    public required string Url { get; set; }

    [Required]
    public required string Name { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<HealthCheckResult> Results { get; set; } = new List<HealthCheckResult>();
}
