using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/v1/urls")]
public class UrlsController : ControllerBase
{
    private readonly UrlShortenerService _urlShortenerService;

    public UrlsController(UrlShortenerService urlShortenerService)
    {
        _urlShortenerService = urlShortenerService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateUrlRequest request,
        CancellationToken cancellationToken)
    {
        var mapping = await _urlShortenerService.CreateAsync(
            request.OriginalUrl,
            cancellationToken);

        return Ok(new
        {
            mapping.Id,
            mapping.ShortCode,
            mapping.OriginalUrl,
            mapping.CreatedAtUtc
        });
    }
}

public record CreateUrlRequest(string OriginalUrl);