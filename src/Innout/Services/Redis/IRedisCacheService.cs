namespace Innout.Services.Redis;

public interface IRedisCacheService
{
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration);

    public Task<T?> GetAsync<T>(string key);

    public Task RemoveAsync(string key);
}