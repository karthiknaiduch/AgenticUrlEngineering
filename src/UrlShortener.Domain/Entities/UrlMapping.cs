namespace UrlShortener.Domain.Entities;

public class UrlMapping
{
    public Guid Id { get; private set; }

    public string ShortCode { get; private set; }

    public string OriginalUrl { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? ExpiresAtUtc { get; private set; }

    public bool IsActive { get; private set; }

    private UrlMapping()
    {
        ShortCode = string.Empty;
        OriginalUrl = string.Empty;
    }

    public UrlMapping(
        string shortCode,
        string originalUrl,
        DateTime? expiresAtUtc = null)
    {
        if (string.IsNullOrWhiteSpace(shortCode))
            throw new ArgumentException(
                "Short code is required.",
                nameof(shortCode));

        if (string.IsNullOrWhiteSpace(originalUrl))
            throw new ArgumentException(
                "Original URL is required.",
                nameof(originalUrl));

        Id = Guid.NewGuid();
        ShortCode = shortCode;
        OriginalUrl = originalUrl;
        CreatedAtUtc = DateTime.UtcNow;
        ExpiresAtUtc = expiresAtUtc;
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsExpired(DateTime utcNow)
    {
        return ExpiresAtUtc.HasValue &&
               ExpiresAtUtc.Value <= utcNow;
    }
}