using Common.BaseTypes;
using Common.DataConverters;
using Common.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace Tomouh.Infrastructure.Features.Cache;

public class InMemoryCacheStrategy : ICacheService
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheStrategy(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(cacheKey, out string? cachedData) && !string.IsNullOrEmpty(cachedData))
        {
            if (typeof(IAuditable).IsAssignableFrom(typeof(T)))
            {
                return Task.FromResult((T?)cachedData.Deserialize(typeof(T)));
            }

            return Task.FromResult(JsonSerializer.Deserialize<T>(cachedData));
        }

        return Task.FromResult<T?>(default);
    }

    public Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            return Task.FromResult(false);
        }

        string jsonData = value is IAuditable auditable
            ? auditable.Serialize()
            : JsonSerializer.Serialize(value);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(3)
        };

        _memoryCache.Set(cacheKey, jsonData, options);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        return Task.FromResult(true);
    }
}

public class InMemoryCacheStrategy<TValue> : ICacheService<TValue>
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheStrategy(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<TValue?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(cacheKey, out string? cachedData) && !string.IsNullOrEmpty(cachedData))
        {
            if (typeof(IAuditable).IsAssignableFrom(typeof(TValue)))
            {
                return Task.FromResult((TValue?)cachedData.Deserialize(typeof(TValue)));
            }

            return Task.FromResult(JsonSerializer.Deserialize<TValue>(cachedData));
        }

        return Task.FromResult<TValue?>(default);
    }

    public Task<bool> SetAsync(string cacheKey, TValue value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            return Task.FromResult(false);
        }

        string jsonData = value is IAuditable auditable
            ? auditable.Serialize()
            : JsonSerializer.Serialize(value);

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(3)
        };

        _memoryCache.Set(cacheKey, jsonData, options);
        return Task.FromResult(true);
    }

    public Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(cacheKey);
        return Task.FromResult(true);
    }
}