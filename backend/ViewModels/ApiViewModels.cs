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
    string? ErrorMessage);
