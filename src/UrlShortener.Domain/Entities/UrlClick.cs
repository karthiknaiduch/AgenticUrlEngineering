namespace UrlShortener.Domain.Entities;

public class UrlClick
{
    public Guid Id { get; private set; }

    public Guid EventId { get; private set; }

    public Guid UrlMappingId { get; private set; }

    public DateTime TimestampUtc { get; private set; }

    public string? IpAddress { get; private set; }

    public string? UserAgent { get; private set; }

    public string? Referrer { get; private set; }

    private UrlClick()
    {
    }

    public UrlClick(
        Guid eventId,
        Guid urlMappingId,
        DateTime timestampUtc,
        string? ipAddress = null,
        string? userAgent = null,
        string? referrer = null)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        UrlMappingId = urlMappingId;
        TimestampUtc = timestampUtc;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        Referrer = referrer;
    }
}