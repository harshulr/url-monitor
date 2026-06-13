using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UrlMonitor.Api.Models;

namespace UrlMonitor.Api.Data;

/// <summary>EF Core context mapping the schema onto SQLite.</summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<MonitoredUrl> MonitoredUrls => Set<MonitoredUrl>();
    public DbSet<SchedulerJob> SchedulerJobs => Set<SchedulerJob>();
    public DbSet<HealthCheckResult> HealthCheckResults => Set<HealthCheckResult>();

    // SQLite drops DateTimeKind; we store UTC, so re-stamp Kind=Utc on read so JSON emits a 'Z'.
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

        modelBuilder.Entity<SchedulerJob>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ExecutedAt).IsRequired();
            entity.Property(e => e.TriggerType).IsRequired();
        });

        modelBuilder.Entity<HealthCheckResult>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Timestamp).IsRequired();
            entity.Property(e => e.ResponseTimeMs).IsRequired();
            entity.Property(e => e.IsSuccess).IsRequired();

            entity.HasOne(e => e.MonitoredUrl)
                  .WithMany(u => u.Results)
                  .HasForeignKey(e => e.MonitoredUrlId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Job)
                  .WithMany(j => j.Results)
                  .HasForeignKey(e => e.JobId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.MonitoredUrlId, e.Timestamp })
                  .HasDatabaseName("IX_HealthCheckResult_MonitoredUrlId_Timestamp");

            entity.HasIndex(e => e.JobId)
                  .HasDatabaseName("IX_HealthCheckResult_JobId");
        });
    }
}
