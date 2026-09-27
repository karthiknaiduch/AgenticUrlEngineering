using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly UrlShortenerDbContext _dbContext;

    public UnitOfWork(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}