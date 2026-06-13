using System.Net;
using UrlMonitor.Api.Models;
using UrlMonitor.Api.Services;
using UrlMonitor.Tests.Support;
using Xunit;

namespace UrlMonitor.Tests;

/// <summary>Covers the core status -> success mapping: healthy, unhealthy, and unreachable.</summary>
public class HealthCheckExecutorTests
{
    private static HealthCheckExecutor Executor(StubHttpMessageHandler handler) =>
        new(new StubHttpClientFactory(handler));

    private static HealthCheckTask SampleTask() =>
        new(Guid.NewGuid(), "https://endpoint.test");

    [Fact]
    public async Task Returns_success_for_2xx()
    {
        var result = await Executor(StubHttpMessageHandler.WithStatus(HttpStatusCode.OK))
            .ExecuteAsync(SampleTask(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(200, result.StatusCode);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task Returns_failure_for_non_2xx()
    {
        var result = await Executor(StubHttpMessageHandler.WithStatus(HttpStatusCode.InternalServerError))
            .ExecuteAsync(SampleTask(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task Records_error_and_null_status_when_unreachable()
    {
        var result = await Executor(StubHttpMessageHandler.Throwing(new HttpRequestException("No such host")))
            .ExecuteAsync(SampleTask(), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Null(result.StatusCode);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }
}
