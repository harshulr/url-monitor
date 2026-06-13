using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using UrlMonitor.Api.Data;
using UrlMonitor.Api.Models;
using UrlMonitor.Api.Services;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Apply migrations, enable WAL (concurrent reads while the worker writes), seed on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    SeedData.EnsureSeeded(db);
}

app.MapGet("/", () => "URL Health Monitor API online.");

app.Run();
