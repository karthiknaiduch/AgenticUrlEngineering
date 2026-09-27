namespace UrlShortener.Application.Events;

public record UrlClickedEvent(
    Guid EventId,
    Guid UrlMappingId,
    string ShortCode,
    DateTime TimestampUtc,
    string? IpAddress,
    string? UserAgent,
    string? Referrer,
    string? CorrelationId = null,
    int RetryCount = 0);