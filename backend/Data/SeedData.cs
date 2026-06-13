using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Data;

/// <summary>Seeds sample URLs on first run, including deterministic failures (404, bad DNS, refused).</summary>
public static class SeedData
{
    public static void EnsureSeeded(AppDbContext db)
    {
        if (db.MonitoredUrls.Any())
            return;

        var now = DateTime.UtcNow;
        db.MonitoredUrls.AddRange(
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Google", Url = "https://www.google.com", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Cloudflare", Url = "https://www.cloudflare.com", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Wikipedia", Url = "https://www.wikipedia.org", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Microsoft", Url = "https://www.microsoft.com", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "MDN Web Docs", Url = "https://developer.mozilla.org", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Example.com", Url = "https://example.com", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "GitHub API", Url = "https://api.github.com", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Google 404", Url = "https://www.google.com/this-page-does-not-exist-xyz", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Bad DNS (no such host)", Url = "https://this-host-does-not-exist.invalid", IsActive = true, CreatedAt = now },
            new MonitoredUrl { Id = Guid.NewGuid(), Name = "Connection refused", Url = "http://localhost:59999", IsActive = true, CreatedAt = now }
        );
        db.SaveChanges();
    }
}
