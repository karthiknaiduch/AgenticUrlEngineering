using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IUrlRepository
{
    Task<UrlMapping?> GetByShortCodeAsync(
        string shortCode,
        CancellationToken cancellationToken = default);

    Task<bool> ShortCodeExistsAsync(
        string shortCode,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        UrlMapping mapping,
        CancellationToken cancellationToken = default);
}