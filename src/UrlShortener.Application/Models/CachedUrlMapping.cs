namespace UrlShortener.Application.Models;

public record CachedUrlMapping(
    Guid Id,
    string ShortCode,
    string OriginalUrl,
    DateTime? ExpiresAtUtc);