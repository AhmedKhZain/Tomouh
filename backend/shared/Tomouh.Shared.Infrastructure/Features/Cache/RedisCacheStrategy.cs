using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Shared.Infrastructure.Features.Cache;

public class RedisCacheStrategy : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer? _connectionMultiplexer;

    public RedisCacheStrategy(
        IDistributedCache distributedCache,
        IConnectionMultiplexer? connectionMultiplexer = null)
    {
        _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        _connectionMultiplexer = connectionMultiplexer;
    }
    public async Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(cacheKey, cancellationToken);

        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }


        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        if (value is null)
        {
            return false;
        }

        string jsonData = JsonSerializer.Serialize(value);

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

    public async Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var lockValue = Guid.NewGuid().ToString();

        // 1. If StackExchange.Redis Native Connection is available (Recommended for Atomic Locks)
        if (_connectionMultiplexer is not null)
        {
            var database = _connectionMultiplexer.GetDatabase();
            var acquired = await database.StringSetAsync(key, lockValue, expiry, When.NotExists);

            if (!acquired)
            {
                return null;
            }

            return new RedisLockHandle(database, key, lockValue);
        }

        // 2. Fallback using IDistributedCache optimistic check
        var existingLock = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (existingLock is not null)
        {
            return null;
        }

        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry
        };

        await _distributedCache.SetStringAsync(key, lockValue, options, cancellationToken);
        return new DistributedCacheLockHandle(_distributedCache, key);
    }


    #region Lock Disposable Handles

    private sealed class RedisLockHandle : IAsyncDisposable
    {
        private readonly IDatabase _database;
        private readonly string _key;
        private readonly string _value;

        public RedisLockHandle(IDatabase database, string key, string value)
        {
            _database = database;
            _key = key;
            _value = value;
        }

        public async ValueTask DisposeAsync()
        {
            const string luaScript = @"
                if redis.call('get', KEYS[1]) == ARGV[1] then
                    return redis.call('del', KEYS[1])
                else
                    return 0
                end";

            await _database.ScriptEvaluateAsync(luaScript, new RedisKey[] { _key }, new RedisValue[] { _value });
        }
    }

    private sealed class DistributedCacheLockHandle : IAsyncDisposable
    {
        private readonly IDistributedCache _cache;
        private readonly string _key;

        public DistributedCacheLockHandle(IDistributedCache cache, string key)
        {
            _cache = cache;
            _key = key;
        }

        public async ValueTask DisposeAsync()
        {
            await _cache.RemoveAsync(_key);
        }
    }

    #endregion

}

