using Microsoft.EntityFrameworkCore;
using UrlMonitor.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Layer 1: Database Core ---------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=urlmonitor.db";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var app = builder.Build();

// Apply migrations, enable WAL (so background writers and API readers don't deadlock on SQLite),
// then seed sample targets on first run.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    SeedData.EnsureSeeded(db);
}

app.MapGet("/", () => "URL Health Monitor API — Layer 1 (Database Core) online.");

app.Run();
