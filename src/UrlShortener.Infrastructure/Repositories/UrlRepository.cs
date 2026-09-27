using Microsoft.EntityFrameworkCore;
using UrlShortener.Application.Interfaces;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Infrastructure.Repositories;

public class UrlRepository : IUrlRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public UrlRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<UrlMapping?> GetByShortCodeAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrlMappings
            .FirstOrDefaultAsync(
                x => x.ShortCode == shortCode,
                cancellationToken);
    }

    public async Task<bool> ShortCodeExistsAsync(
        string shortCode,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.UrlMappings
            .AnyAsync(
                x => x.ShortCode == shortCode,
                cancellationToken);
    }

    public async Task AddAsync(
        UrlMapping mapping,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.UrlMappings.AddAsync(
            mapping,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}