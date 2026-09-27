using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

namespace UrlShortener.IntegrationTests;

public class UrlShortenerApiTests
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UrlShortenerApiTests(
        WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateUrl_ShouldPersistAndReturnShortCode()
    {
        // Arrange
        var request = new
        {
            originalUrl = "https://example.com/integration-test"
        };

        // Act
        var response =
            await _client.PostAsJsonAsync(
                "/api/v1/urls",
                request);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var result =
            await response.Content
                .ReadFromJsonAsync<CreateUrlResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(
            Guid.Empty,
            result.Id);
        Assert.False(
            string.IsNullOrWhiteSpace(result.ShortCode));
        Assert.Equal(
            request.originalUrl,
            result.OriginalUrl);
    }

    private sealed record CreateUrlResponse(
        Guid Id,
        string ShortCode,
        string OriginalUrl,
        DateTime CreatedAtUtc);
}