using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Services;

/// <summary>Queues a check task for every active URL. Shared by the scheduler and the manual sync endpoint.</summary>
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

    /// <summary>Enqueues one task per active URL. Returns how many were queued.</summary>
    public async Task<int> QueueActiveChecksAsync(CancellationToken ct = default)
    {
        var active = await _db.MonitoredUrls
            .Where(u => u.IsActive)
            .Select(u => new { u.Id, u.Url })
            .ToListAsync(ct);

        foreach (var u in active)
            await _channel.Writer.WriteAsync(new HealthCheckTask(u.Id, u.Url), ct);

        if (active.Count > 0)
            _logger.LogInformation("Queued {Count} check(s).", active.Count);

        return active.Count;
    }
}
