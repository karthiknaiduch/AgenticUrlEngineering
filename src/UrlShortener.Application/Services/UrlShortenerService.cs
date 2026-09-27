using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Services;

public class UrlShortenerService
{
    private readonly IUrlRepository _repository;

    public UrlShortenerService(IUrlRepository repository)
    {
        _repository = repository;
    }

    public async Task<UrlMapping> CreateAsync(
        string originalUrl,
        CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(
                originalUrl,
                UriKind.Absolute,
                out var uri))
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

        return mapping;
    }

    private static string GenerateShortCode()
    {
        return Convert.ToBase64String(
                Guid.NewGuid().ToByteArray())
            .Replace("/", "")
            .Replace("+", "")
            .Replace("=", "")
            [..7];
    }
}