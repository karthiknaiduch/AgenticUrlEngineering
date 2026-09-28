using UrlShortener.Analytics.Api.Models;

namespace UrlShortener.Analytics.Api.Services;

public interface IAnalyticsService
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
