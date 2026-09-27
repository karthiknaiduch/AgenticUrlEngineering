using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Interfaces;

public interface IOutboxRepository
{
    Task AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default);
}