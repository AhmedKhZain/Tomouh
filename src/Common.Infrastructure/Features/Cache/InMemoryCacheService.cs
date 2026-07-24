using Common.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Common.Infrastructure.Features.Cache;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        var result = _memoryCache.Get<string>(cacheKey);

        if (result == null)
        {
            return Task.FromResult<T?>(default);
        }

        return Task.FromResult(JsonSerializer.Deserialize<T>(result));
    }

    public Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var jsonValue = JsonSerializer.Serialize(value);

        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
        }

        _memoryCache.Set(cacheKey, jsonValue, options);

        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);

        return Task.CompletedTask;
    }
}
