using UrlShortener.Analytics.Api.Models;

namespace UrlShortener.Analytics.Api.Repositories;

public interface IAnalyticsRepository
{
    Task<AnalyticsSummary?> GetSummaryAsync(
        string shortCode,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<UrlClickDto>?> GetClicksAsync(
        string shortCode,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken);
}
