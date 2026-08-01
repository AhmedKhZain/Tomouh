using Common.BaseTypes;
using Common.DataConverters;
using Common.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Tomouh.Infrastructure.Features.Cache;

public class RedisCacheStrategy : ICacheService
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheStrategy(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        if (typeof(IAuditable).IsAssignableFrom(typeof(T)))
        {
            return (T?)cachedData.Deserialize(typeof(T));
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            return false;
        }

        string jsonData = value is IAuditable auditable
            ? auditable.Serialize()
            : JsonSerializer.Serialize(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(3)
        };

        await _distributedCache.SetStringAsync(cacheKey, jsonData, options, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(key, cancellationToken);
        return true;
    }
}

public class RedisCacheStrategy<T> : ICacheService<T>
{
    private readonly IDistributedCache _distributedCache;

    public RedisCacheStrategy(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    public async Task<T?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        if (typeof(IAuditable).IsAssignableFrom(typeof(T)))
        {
            return (T?)cachedData.Deserialize(typeof(T));
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task<bool> SetAsync(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            return false;
        }

        string jsonData = value is IAuditable auditable
            ? auditable.Serialize()
            : JsonSerializer.Serialize(value);

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(3)
        };

        await _distributedCache.SetStringAsync(cacheKey, jsonData, options, cancellationToken);
        return true;
    }

    public async Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(cacheKey, cancellationToken);
        return true;
    }
}