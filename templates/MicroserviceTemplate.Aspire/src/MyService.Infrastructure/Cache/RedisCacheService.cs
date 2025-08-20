using Microsoft.Extensions.Configuration;
using MyService.Domain.Interfaces;
using StackExchange.Redis;
using System.Text.Json;

namespace MyService.Infrastructure.Cache;

/// <summary>
/// Redis cache service implementation.
/// </summary>
public class RedisCacheService : ICacheService
{
    /// <summary>
    /// Redis cache database.
    /// </summary>
    private readonly IDatabase _db;

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheService"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer.</param>
    /// <exception cref="ArgumentNullException">Thrown when the Redis connection multiplexer is not configured.</exception>
    public RedisCacheService(IConnectionMultiplexer redis)
    {
        if (redis == null)
        {
            throw new ArgumentNullException(nameof(redis), "Redis connection multiplexer is not configured");
        }
        _db = redis.GetDatabase();
    }

    /// <summary>
    /// Sets the response in the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="response">The response to cache.</param>
    /// <param name="expiry">The cache expiry duration.</param>
    public async Task SetResponseAsync<TResponse>(string key, TResponse response, TimeSpan? expiry = null)
    {
        var json = JsonSerializer.Serialize(response);
        await _db.StringSetAsync(key, json, expiry);
    }

    /// <summary>
    /// Gets the response from the cache.
    /// </summary>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached response or null if not found.</returns>
    public async Task<TResponse?> GetResponseAsync<TResponse>(string key)
    {
        var json = await _db.StringGetAsync(key);
        if (json.IsNullOrEmpty) return default;
        return JsonSerializer.Deserialize<TResponse>(json!);
    }

    /// <summary>
    /// Removes the response from the cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    public async Task RemoveResponseAsync(string key)
    {
        await _db.KeyDeleteAsync(key);
    }
}