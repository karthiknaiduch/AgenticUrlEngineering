using Microsoft.AspNetCore.Mvc;
using UrlShortener.Analytics.Api.Services;

namespace UrlShortener.Analytics.Api.Controllers;

[ApiController]
[Route("api/v1/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service)
    {
        _service = service;
    }

    [HttpGet("{shortCode}/summary")]
    public async Task<IActionResult> GetSummary(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(
            shortCode,
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{shortCode}/clicks")]
    public async Task<IActionResult> GetClicks(
        string shortCode,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetClicksAsync(
            shortCode,
            from,
            to,
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{shortCode}")]
    public async Task<IActionResult> GetAnalytics(
        string shortCode,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSummaryAsync(
            shortCode,
            cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
