using System.Diagnostics;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Services;

/// <summary>Runs one HTTP check and evaluates it into a HealthCheckResult. Unit-testable in isolation.</summary>
public class HealthCheckExecutor
{
    public const string HttpClientName = "HealthCheckClient";

    private readonly IHttpClientFactory _httpClientFactory;

    public HealthCheckExecutor(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Pings the target, timing it. Records a result for any response (incl. 4xx/5xx) or transport
    /// failure. Rethrows only when <paramref name="ct"/> is cancelled (shutdown).
    /// </summary>
    public async Task<HealthCheckResult> ExecuteAsync(HealthCheckTask task, CancellationToken ct)
    {
        var client = _httpClientFactory.CreateClient(HttpClientName);
        var stopwatch = Stopwatch.StartNew();

        var result = new HealthCheckResult
        {
            Id = Guid.NewGuid(),
            JobId = task.JobId,
            MonitoredUrlId = task.UrlId,
            Timestamp = DateTime.UtcNow,
        };

        try
        {
            // ResponseHeadersRead: don't buffer the body — we only need status + latency.
            using var response = await client.GetAsync(
                task.TargetUrl, HttpCompletionOption.ResponseHeadersRead, ct);
            stopwatch.Stop();

            result.StatusCode = (int)response.StatusCode;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = response.IsSuccessStatusCode;
            result.Timestamp = DateTime.UtcNow;
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested)
        {
            throw; // app shutdown, not a failure — let the caller stop the loop
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or OperationCanceledException)
        {
            // No response: timeout (token not cancelled), DNS failure, connection refused, etc.
            stopwatch.Stop();
            result.StatusCode = null;
            result.ResponseTimeMs = stopwatch.ElapsedMilliseconds;
            result.IsSuccess = false;
            result.ErrorMessage = ex.Message;
            result.Timestamp = DateTime.UtcNow;
        }

        return result;
    }
}
