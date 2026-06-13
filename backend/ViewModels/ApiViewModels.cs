namespace UrlMonitor.Api.ViewModels;

/// <summary>Current health state for a URL (latest result flattened in).</summary>
public record UrlStatusViewModel(
    Guid Id,
    string Name,
    string Url,
    bool IsActive,
    int? LastStatusCode,
    bool? LastIsSuccess,
    long? LastResponseTimeMs,
    DateTime? LastCheckedAt,
    string? LastErrorMessage);

/// <summary>One past check for a URL's history.</summary>
public record HistoryItemViewModel(
    Guid Id,
    DateTime Timestamp,
    int? StatusCode,
    long ResponseTimeMs,
    bool IsSuccess,
    string? ErrorMessage,
    string TriggerType);

/// <summary>Summary of one past run for the Job History list.</summary>
public record JobSummaryViewModel(
    Guid Id,
    DateTime ExecutedAt,
    string TriggerType,
    int TotalChecks,
    int SuccessCount,
    int FailureCount);

/// <summary>One endpoint's result within a run (shown when a job is expanded).</summary>
public record JobResultViewModel(
    Guid ResultId,
    string Name,
    string Url,
    int? StatusCode,
    long ResponseTimeMs,
    bool IsSuccess,
    string? ErrorMessage);
