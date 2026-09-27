using System.Text.Json;
using StackExchange.Redis;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Cache;

public class RedisUrlCache : IUrlCache
{
    private readonly IDatabase _database;

    public RedisUrlCache(IConnectionMultiplexer redis)
    {
        _database = redis.GetDatabase();
    }

    public async Task<T?> GetAsync<T>(
        string key,
        CancellationToken cancellationToken = default)
    {
        var value = await _database.StringGetAsync(key);

        if (!value.HasValue)
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(
            value.ToString());
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan expiration,
        CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(value);

        await _database.StringSetAsync(
            key,
            json,
            expiration);
    }

    public async Task RemoveAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _database.KeyDeleteAsync(key);
    }
}