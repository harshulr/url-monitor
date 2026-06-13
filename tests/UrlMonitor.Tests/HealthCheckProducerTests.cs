using System.Threading.Channels;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;
using UrlMonitor.Api.Services;
using Xunit;

namespace UrlMonitor.Tests;

/// <summary>Verifies the producer records a run and queues a task per active URL, against real in-memory SQLite.</summary>
public class HealthCheckProducerTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;

    public HealthCheckProducerTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private void AddUrl(string name, bool isActive) =>
        _db.MonitoredUrls.Add(new MonitoredUrl
        {
            Id = Guid.NewGuid(),
            Name = name,
            Url = $"https://{name}.test",
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow,
        });

    private static int Drain(Channel<HealthCheckTask> channel)
    {
        var count = 0;
        while (channel.Reader.TryRead(out _)) count++;
        return count;
    }

    [Fact]
    public async Task Records_job_and_queues_one_task_per_active_url()
    {
        AddUrl("active1", true);
        AddUrl("active2", true);
        AddUrl("inactive", false);
        await _db.SaveChangesAsync();

        var channel = Channel.CreateUnbounded<HealthCheckTask>();
        var producer = new HealthCheckProducer(_db, channel, NullLogger<HealthCheckProducer>.Instance);

        var jobId = await producer.QueueActiveChecksAsync(TriggerTypes.Scheduled);

        Assert.NotNull(jobId);
        var job = Assert.Single(_db.SchedulerJobs);
        Assert.Equal(TriggerTypes.Scheduled, job.TriggerType);
        Assert.Equal(2, Drain(channel));
    }

    [Fact]
    public async Task Returns_null_and_queues_nothing_when_no_active_urls()
    {
        AddUrl("inactive", false);
        await _db.SaveChangesAsync();

        var channel = Channel.CreateUnbounded<HealthCheckTask>();
        var producer = new HealthCheckProducer(_db, channel, NullLogger<HealthCheckProducer>.Instance);

        var jobId = await producer.QueueActiveChecksAsync(TriggerTypes.Manual);

        Assert.Null(jobId);
        Assert.Empty(_db.SchedulerJobs);
        Assert.Equal(0, Drain(channel));
    }
}
