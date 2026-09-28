namespace UrlShortener.Analytics.Api.Models;

public class AnalyticsSummary
{
    public string ShortCode { get; set; } = string.Empty;
    public long TotalClicks { get; set; }
    public long UniqueVisitors { get; set; }
    public DateTime? LastClickedAt { get; set; }
}
