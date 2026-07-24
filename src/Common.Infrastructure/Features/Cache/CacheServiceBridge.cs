using Common.Services;

namespace Common.Infrastructure.Features.Cache
{
    public class CacheServiceBridge<TValue> : ICacheService<TValue>
    {
        private readonly ICacheService<TValue> _innerCacheService;

        public CacheServiceBridge(CacheServiceFactory factory)
        {
            _innerCacheService = factory.GetCacheService<TValue>();
        }

        public Task<TValue?> GetAsync(string cacheKey)
        {
            return _innerCacheService.GetAsync(cacheKey);
        }

        public Task SetAsync<TValue>(string cacheKey, TValue value, TimeSpan? expiration)
        {
            return _innerCacheService.SetAsync(cacheKey, value, expiration);
        }
    }

    public class CacheServiceBridge : ICacheService
    {
        private readonly ICacheService _innerCacheService;

        public CacheServiceBridge(CacheServiceFactory factory)
        {
            _innerCacheService = factory.GetCacheService();
        }

        public Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.GetAsync<T>(cacheKey, cancellationToken);
        }

        public Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.SetAsync(cacheKey, value, expiration, cancellationToken);
        }

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.RemoveAsync(key, cancellationToken);
        }
    }
}
