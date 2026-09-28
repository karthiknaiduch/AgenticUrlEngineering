using Microsoft.EntityFrameworkCore;
using UrlShortener.Analytics.Api.Models;
using UrlShortener.Infrastructure.Data;

namespace UrlShortener.Analytics.Api.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly UrlShortenerDbContext _dbContext;

    public AnalyticsRepository(UrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AnalyticsSummary?> GetSummaryAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var urlMapping = await _dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ShortCode == shortCode,
                cancellationToken);

        if (urlMapping == null)
            return null;

        var clicks = _dbContext.UrlClicks
            .AsNoTracking()
            .Where(x => x.UrlMappingId == urlMapping.Id);

        var totalClicks = await clicks.LongCountAsync(
            cancellationToken);

        var uniqueVisitors = await clicks
            .Where(x => x.IpAddress != null)
            .Select(x => x.IpAddress)
            .Distinct()
            .LongCountAsync(cancellationToken);

        var lastClickedAt = await clicks
            .Select(x => (DateTime?)x.TimestampUtc)
            .MaxAsync(cancellationToken);

        return new AnalyticsSummary
        {
            ShortCode = urlMapping.ShortCode,
            TotalClicks = totalClicks,
            UniqueVisitors = uniqueVisitors,
            LastClickedAt = lastClickedAt
        };
    }

    public async Task<IReadOnlyList<UrlClickDto>?> GetClicksAsync(
        string shortCode,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        var urlMapping = await _dbContext.UrlMappings
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.ShortCode == shortCode,
                cancellationToken);

        if (urlMapping == null)
            return null;

        var query = _dbContext.UrlClicks
            .AsNoTracking()
            .Where(x => x.UrlMappingId == urlMapping.Id);

        if (from.HasValue)
        {
            query = query.Where(
                x => x.TimestampUtc >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(
                x => x.TimestampUtc <= to.Value);
        }

        return await query
            .OrderByDescending(x => x.TimestampUtc)
            .Select(x => new UrlClickDto
            {
                EventId = x.EventId,
                TimestampUtc = x.TimestampUtc,
                IpAddress = x.IpAddress,
                UserAgent = x.UserAgent,
                Referrer = x.Referrer
            })
            .ToListAsync(cancellationToken);
    }
}
