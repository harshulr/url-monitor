using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Services;

/// <summary>Queues a check batch every 60s via PeriodicTimer (and once immediately on startup).</summary>
public class SchedulerWorker : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(60);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SchedulerWorker> _logger;

    public SchedulerWorker(IServiceScopeFactory scopeFactory, ILogger<SchedulerWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("SchedulerWorker started; batching every {Seconds}s.", Interval.TotalSeconds);

        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var producer = scope.ServiceProvider.GetRequiredService<HealthCheckProducer>();
                await producer.QueueActiveChecksAsync(TriggerTypes.Scheduled, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled batch failed; will retry next tick.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
