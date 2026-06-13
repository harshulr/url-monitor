using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;
using UrlMonitor.Api.Services;
using UrlMonitor.Api.ViewModels;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicy = "FrontendCors";

// Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=urlmonitor.db";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Channel engine: producers write tasks, one ChannelConsumerWorker drains them.
builder.Services.AddSingleton(Channel.CreateUnbounded<HealthCheckTask>(new UnboundedChannelOptions
{
    SingleReader = true,
    AllowSynchronousContinuations = false
}));

// User-Agent set so servers that 403 UA-less requests (Wikipedia, GitHub API) aren't false negatives.
builder.Services.AddHttpClient(HealthCheckExecutor.HttpClientName, client =>
{
    client.Timeout = TimeSpan.FromSeconds(10);
    client.DefaultRequestHeaders.UserAgent.ParseAdd("UrlHealthMonitor/1.0 (+health-check)");
});

builder.Services.AddSingleton<HealthCheckExecutor>();
builder.Services.AddHostedService<ChannelConsumerWorker>();

// Scheduler + producer
builder.Services.AddScoped<HealthCheckProducer>();
builder.Services.AddHostedService<SchedulerWorker>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicy, policy =>
        policy.SetIsOriginAllowed(origin =>
                  Uri.TryCreate(origin, UriKind.Absolute, out var uri) && uri.IsLoopback)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// Apply migrations, enable WAL (concurrent reads while the worker writes), seed on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    SeedData.EnsureSeeded(db);
}

app.UseCors(CorsPolicy);

// Current health state per URL (latest result joined).
app.MapGet("/api/urls", async (AppDbContext db) =>
{
    var rows = await db.MonitoredUrls
        .OrderBy(u => u.Name)
        .Select(u => new
        {
            u.Id,
            u.Name,
            u.Url,
            u.IsActive,
            Latest = db.HealthCheckResults
                .Where(r => r.MonitoredUrlId == u.Id)
                .OrderByDescending(r => r.Timestamp)
                .FirstOrDefault()
        })
        .ToListAsync();

    var result = rows.Select(u => new UrlStatusViewModel(
        u.Id, u.Name, u.Url, u.IsActive,
        u.Latest?.StatusCode,
        u.Latest != null ? u.Latest.IsSuccess : null,
        u.Latest != null ? u.Latest.ResponseTimeMs : null,
        u.Latest != null ? u.Latest.Timestamp : null,
        u.Latest?.ErrorMessage));

    return Results.Ok(result);
});

// Chronological check log for one URL.
app.MapGet("/api/urls/{id:guid}/history", async (Guid id, AppDbContext db) =>
{
    if (!await db.MonitoredUrls.AnyAsync(u => u.Id == id))
        return Results.NotFound();

    var history = await db.HealthCheckResults
        .Where(r => r.MonitoredUrlId == id)
        .OrderByDescending(r => r.Timestamp)
        .Take(100)
        .Select(r => new HistoryItemViewModel(
            r.Id, r.Timestamp, r.StatusCode, r.ResponseTimeMs, r.IsSuccess, r.ErrorMessage))
        .ToListAsync();

    return Results.Ok(history);
});

// Manual trigger: queue a check batch, return 202 immediately.
app.MapPost("/api/urls/sync", async (HealthCheckProducer producer) =>
{
    var queued = await producer.QueueActiveChecksAsync();
    return Results.Accepted(value: new { message = "Health sync queued.", queued });
});

app.MapGet("/", () => "URL Health Monitor API online.");

app.Run();
