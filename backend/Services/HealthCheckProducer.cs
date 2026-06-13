using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Services;

/// <summary>Records a run (SchedulerJob) and queues a check task per active URL. Shared by scheduler + manual sync.</summary>
public class HealthCheckProducer
{
    private readonly AppDbContext _db;
    private readonly Channel<HealthCheckTask> _channel;
    private readonly ILogger<HealthCheckProducer> _logger;

    public HealthCheckProducer(AppDbContext db, Channel<HealthCheckTask> channel, ILogger<HealthCheckProducer> logger)
    {
        _db = db;
        _channel = channel;
        _logger = logger;
    }

    /// <summary>Creates a job of the given trigger type and enqueues one task per active URL. Returns the job id, or null if none active.</summary>
    public async Task<Guid?> QueueActiveChecksAsync(string triggerType, CancellationToken ct = default)
    {
        var active = await _db.MonitoredUrls
            .Where(u => u.IsActive)
            .Select(u => new { u.Id, u.Url })
            .ToListAsync(ct);

        if (active.Count == 0)
        {
            _logger.LogInformation("No active URLs to queue for {Trigger} run.", triggerType);
            return null;
        }

        var job = new SchedulerJob
        {
            Id = Guid.NewGuid(),
            ExecutedAt = DateTime.UtcNow,
            TriggerType = triggerType,
        };
        _db.SchedulerJobs.Add(job);
        await _db.SaveChangesAsync(ct);

        foreach (var u in active)
            await _channel.Writer.WriteAsync(new HealthCheckTask(job.Id, u.Id, u.Url), ct);

        _logger.LogInformation("Queued {Trigger} job {JobId} with {Count} check(s).", triggerType, job.Id, active.Count);
        return job.Id;
    }
}
