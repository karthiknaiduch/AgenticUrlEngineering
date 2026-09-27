using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Application.Events;
using UrlShortener.Application.Models;
//using UrlShortener.Application.Interfaces;

namespace UrlShortener.Application.Services;

public class UrlShortenerService
{
    private readonly IUrlRepository _repository;
    private readonly IUrlCache _cache;
    private readonly IOutboxRepository _outboxRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEventPublisher _eventPublisher;

    public UrlShortenerService(
        IUrlRepository repository,
        IUrlCache cache,
        IEventPublisher eventPublisher,
        IOutboxRepository outboxRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _cache = cache;
        _eventPublisher =eventPublisher;
        _outboxRepository= outboxRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UrlMapping> CreateAsync(
        string originalUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(originalUrl, UriKind.Absolute, out var uri))
        {
            throw new ArgumentException(
                "Invalid URL.",
                nameof(originalUrl));
        }

        if (uri.Scheme != Uri.UriSchemeHttp &&
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new ArgumentException(
                "Only HTTP and HTTPS URLs are supported.",
                nameof(originalUrl));
        }

        var shortCode = GenerateShortCode();

        while (await _repository.ShortCodeExistsAsync(
            shortCode,
            cancellationToken))
        {
            shortCode = GenerateShortCode();
        }

        var mapping = new UrlMapping(
            shortCode,
            originalUrl);

        await _repository.AddAsync(
    mapping,
    cancellationToken);

await _unitOfWork.SaveChangesAsync(
    cancellationToken);

return mapping;
    }

    public async Task<string?> GetOriginalUrlAsync(
    string shortCode,
    CancellationToken cancellationToken = default)
{
    // 1. Redis
    var cachedMapping =
        await _cache.GetAsync<CachedUrlMapping>(
            shortCode,
            cancellationToken);

    if (cachedMapping is not null)
    {
        if (cachedMapping.ExpiresAtUtc.HasValue &&
            cachedMapping.ExpiresAtUtc.Value <= DateTime.UtcNow)
        {
            await _cache.RemoveAsync(
                shortCode,
                cancellationToken);

            return null;
        }

        await PublishClickEventAsync(
            cachedMapping,
            cancellationToken);

        return cachedMapping.OriginalUrl;
    }

    // 2. PostgreSQL
    var mapping =
        await _repository.GetByShortCodeAsync(
            shortCode,
            cancellationToken);

    if (mapping is null)
    {
        return null;
    }

    // 3. Validate
    if (!mapping.IsActive ||
        mapping.IsExpired(DateTime.UtcNow))
    {
        return null;
    }

    // 4. Populate Redis
    var cacheEntry = new CachedUrlMapping(
        mapping.Id,
        mapping.ShortCode,
        mapping.OriginalUrl,
        mapping.ExpiresAtUtc);

    await _cache.SetAsync(
        mapping.ShortCode,
        cacheEntry,
        TimeSpan.FromMinutes(30),
        cancellationToken);

    // 5. Publish analytics event
    await PublishClickEventAsync(
        cacheEntry,
        cancellationToken);

    return mapping.OriginalUrl;
}

    private static string GenerateShortCode()
    {
        return Convert.ToBase64String(
                Guid.NewGuid().ToByteArray())
            .Replace("/", "")
            .Replace("+", "")
            .Replace("=", "")[..7];
    }
    private async Task PublishClickEventAsync(
    CachedUrlMapping mapping,
    CancellationToken cancellationToken)
{
    var clickEvent = new UrlClickedEvent(
        Guid.NewGuid(),
        mapping.Id,
        mapping.ShortCode,
        DateTime.UtcNow,
        null,
        null,
        null,
        null,
        0);

    var payload = System.Text.Json.JsonSerializer.Serialize(
        clickEvent);

    var outboxMessage = new OutboxMessage(
        "UrlClicked",
        payload);

    await _outboxRepository.AddAsync(
        outboxMessage,
        cancellationToken);

        await _unitOfWork.SaveChangesAsync(
        cancellationToken);
}
}

