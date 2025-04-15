using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace Innout.Services.Redis;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    private readonly DistributedCacheEntryOptions _defaultOptions;

    public RedisCacheService(IDistributedCache cache)
    {
        _cache = cache;
        _defaultOptions = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
        };
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var json = JsonSerializer.Serialize(value);
        var options = expiration is not null ?
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration
            } : _defaultOptions;

        await _cache.SetStringAsync(key, json, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var json = await _cache.GetStringAsync(key);
        return json is null ? default : JsonSerializer.Deserialize<T>(json);
    }

    public async Task RemoveAsync(string key)
    {
        await _cache.RefreshAsync(key);
    }
}