using Common.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Common.Infrastructure.Features.Cache;

public class InMemoryCacheStrategy<TValue> : ICacheService<TValue>
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheStrategy(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<TValue?> GetAsync(string cacheKey)
    {
        var result = _memoryCache.Get<string>(cacheKey);
        if (result == null)
            return Task.FromResult<TValue?>(default);
        return Task.FromResult(JsonSerializer.Deserialize<TValue>(result));
    }


    public Task SetAsync<TValue1>(string cacheKey, TValue1 value, TimeSpan? expiration)
    {
        var options = new MemoryCacheEntryOptions();

        if (expiration.HasValue)
        {
            options.SetAbsoluteExpiration(expiration.Value);
        }
        else
        {
            options.SetAbsoluteExpiration(TimeSpan.FromHours(1));
        }
        var jsonValue = JsonSerializer.Serialize(value);

        _memoryCache.Set(cacheKey, jsonValue, options);

        return Task.CompletedTask;
    }
}
