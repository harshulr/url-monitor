using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Data;

/// <summary>EF Core context. Maps the schema onto SQLite.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredUrl> MonitoredUrls => Set<MonitoredUrl>();
    public DbSet<HealthCheckResult> HealthCheckResults => Set<HealthCheckResult>();

    // SQLite has no DateTimeKind, so values round-trip as Unspecified and serialize without a 'Z'.
    // We store UTC everywhere, so force Kind=Utc on read; JSON then emits an ISO-8601 'Z' and the
    // frontend's toLocaleString() renders the user's local time correctly.
    private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
    {
        public UtcDateTimeConverter()
            : base(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
        {
        }
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MonitoredUrl>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Url).IsRequired();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IsActive).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
        });

        modelBuilder.Entity<HealthCheckResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.ResponseTimeMs).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired();
            // StatusCode and ErrorMessage are intentionally nullable.

            entity.HasOne(e => e.MonitoredUrl)
                  .WithMany(u => u.Results)
                  .HasForeignKey(e => e.MonitoredUrlId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Optimizes chronological history queries for one URL.
            entity.HasIndex(e => new { e.MonitoredUrlId, e.Timestamp })
                  .HasDatabaseName("IX_HealthCheckResult_MonitoredUrlId_Timestamp");
        });
    }
}
