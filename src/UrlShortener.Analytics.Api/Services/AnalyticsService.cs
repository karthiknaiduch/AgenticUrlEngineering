using UrlShortener.Analytics.Api.Models;
using UrlShortener.Analytics.Api.Repositories;

namespace UrlShortener.Analytics.Api.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _repository;

    public AnalyticsService(IAnalyticsRepository repository)
    {
        _repository = repository;
    }

    public Task<AnalyticsSummary?> GetSummaryAsync(
        string shortCode,
        CancellationToken cancellationToken)
    {
        return _repository.GetSummaryAsync(
            shortCode,
            cancellationToken);
    }

    public Task<IReadOnlyList<UrlClickDto>?> GetClicksAsync(
        string shortCode,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken)
    {
        return _repository.GetClicksAsync(
            shortCode,
            from,
            to,
            cancellationToken);
    }
}
