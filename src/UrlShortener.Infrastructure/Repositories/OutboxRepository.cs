using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public OutboxRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        OutboxMessage message,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.OutboxMessages.AddAsync(
            message,
            cancellationToken);
    }
}