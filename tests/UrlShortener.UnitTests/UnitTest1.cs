using Moq;
using UrlShortener.Application.Interfaces;
using UrlShortener.Application.Models;
using UrlShortener.Application.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.UnitTests;

public class UrlShortenerServiceTests
{
    private readonly Mock<IUrlRepository> _repositoryMock;
    private readonly Mock<IUrlCache> _cacheMock;
    private readonly Mock<IOutboxRepository> _outboxMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;

    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly UrlShortenerService _service;

    public UrlShortenerServiceTests()
    {
        _repositoryMock = new Mock<IUrlRepository>();
        _cacheMock = new Mock<IUrlCache>();
        _outboxMock = new Mock<IOutboxRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _service = new UrlShortenerService(
        _repositoryMock.Object,
        _cacheMock.Object,
        _eventPublisherMock.Object,
        _outboxMock.Object,
        _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndPersistUrl()
    {
        // Arrange
        var originalUrl = "https://example.com";

        _repositoryMock
            .Setup(x => x.ShortCodeExistsAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(x => x.AddAsync(
                It.IsAny<UrlMapping>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _service.CreateAsync(originalUrl);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(originalUrl, result.OriginalUrl);
        Assert.True(result.IsActive);
        Assert.False(string.IsNullOrWhiteSpace(result.ShortCode));

        _repositoryMock.Verify(
            x => x.AddAsync(
                It.IsAny<UrlMapping>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldRejectInvalidUrl()
    {
        // Arrange
        var invalidUrl = "not-a-url";

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateAsync(invalidUrl));
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnCachedUrl()
    {
        // Arrange
        var mapping = new CachedUrlMapping(
            Guid.NewGuid(),
            "abc123",
            "https://example.com",
            null);

        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "abc123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        // Act
        var result = await _service.GetOriginalUrlAsync("abc123");

        // Assert
        Assert.Equal(
            "https://example.com",
            result);

        _repositoryMock.Verify(
            x => x.GetByShortCodeAsync(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnNullWhenUrlDoesNotExist()
    {
        // Arrange
        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "missing",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedUrlMapping?)null);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(
                "missing",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((UrlMapping?)null);

        // Act
        var result =
            await _service.GetOriginalUrlAsync("missing");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnNullForExpiredUrl()
    {
        // Arrange
        var mapping = new CachedUrlMapping(
            Guid.NewGuid(),
            "expired",
            "https://example.com",
            DateTime.UtcNow.AddMinutes(-5));

        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "expired",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        // Act
        var result =
            await _service.GetOriginalUrlAsync("expired");

        // Assert
        Assert.Null(result);

        _cacheMock.Verify(
            x => x.RemoveAsync(
                "expired",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldReturnNullForInactiveUrl()
    {
        // Arrange
        var mapping =
            new UrlMapping(
                "inactive",
                "https://example.com");

        mapping.Deactivate();

        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "inactive",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedUrlMapping?)null);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(
                "inactive",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        // Act
        var result =
            await _service.GetOriginalUrlAsync("inactive");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldCacheDatabaseResult()
    {
        // Arrange
        var mapping =
            new UrlMapping(
                "db123",
                "https://example.com");

        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "db123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((CachedUrlMapping?)null);

        _repositoryMock
            .Setup(x => x.GetByShortCodeAsync(
                "db123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        // Act
        var result =
            await _service.GetOriginalUrlAsync("db123");

        // Assert
        Assert.Equal(
            "https://example.com",
            result);

        _cacheMock.Verify(
            x => x.SetAsync(
                "db123",
                It.IsAny<CachedUrlMapping>(),
                It.IsAny<TimeSpan>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetOriginalUrlAsync_ShouldCreateOutboxMessage()
    {
        // Arrange
        var mapping = new CachedUrlMapping(
            Guid.NewGuid(),
            "analytics",
            "https://example.com",
            null);

        _cacheMock
            .Setup(x => x.GetAsync<CachedUrlMapping>(
                "analytics",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(mapping);

        // Act
        await _service.GetOriginalUrlAsync("analytics");

        // Assert
        _outboxMock.Verify(
            x => x.AddAsync(
                It.Is<OutboxMessage>(message =>
                    message.EventType == "UrlClicked"),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}