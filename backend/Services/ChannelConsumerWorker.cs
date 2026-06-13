using System.Threading.Channels;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Services;

/// <summary>Drains the task channel, runs each check via HealthCheckExecutor, and persists the result.</summary>
public class ChannelConsumerWorker : BackgroundService
{
    private readonly Channel<HealthCheckTask> _channel;
    private readonly HealthCheckExecutor _executor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ChannelConsumerWorker> _logger;

    public ChannelConsumerWorker(
        Channel<HealthCheckTask> channel,
        HealthCheckExecutor executor,
        IServiceScopeFactory scopeFactory,
        ILogger<ChannelConsumerWorker> logger)
    {
        _channel = channel;
        _executor = executor;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ChannelConsumerWorker started; awaiting health check tasks.");

        await foreach (var task in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                var result = await _executor.ExecuteAsync(task, stoppingToken);
                await PersistAsync(result, stoppingToken);

                _logger.LogInformation(
                    "Checked {Url} -> status {Status}, {Elapsed}ms, success {Success}",
                    task.TargetUrl, result.StatusCode, result.ResponseTimeMs, result.IsSuccess);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // One bad task must never kill the loop.
                _logger.LogError(ex, "Unhandled error processing task for {Url}", task.TargetUrl);
            }
        }
    }

    /// <summary>Saves a result using a fresh scope (DbContext is scoped, this worker is a singleton).</summary>
    private async Task PersistAsync(HealthCheckResult result, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.HealthCheckResults.Add(result);
        await db.SaveChangesAsync(ct);
    }
}
