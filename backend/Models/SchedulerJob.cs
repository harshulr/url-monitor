using System.ComponentModel.DataAnnotations;

namespace UrlMonitor.Api.Models;

/// <summary>One run cycle (batch) of checks, tagged by what triggered it.</summary>
public class SchedulerJob
{
    public Guid Id { get; set; }

    public DateTime ExecutedAt { get; set; }

    [Required]
    public required string TriggerType { get; set; }

    public ICollection<HealthCheckResult> Results { get; set; } = new List<HealthCheckResult>();
}

/// <summary>Allowed values for <see cref="SchedulerJob.TriggerType"/>.</summary>
public static class TriggerTypes
{
    public const string Scheduled = "Scheduled";
    public const string Manual = "Manual";
}
