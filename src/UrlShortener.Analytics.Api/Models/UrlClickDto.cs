namespace UrlShortener.Analytics.Api.Models;

public class UrlClickDto
{
    public Guid EventId { get; set; }
    public DateTime TimestampUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referrer { get; set; }
}
