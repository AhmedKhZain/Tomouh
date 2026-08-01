using Common.Services;

namespace Tomouh.Infrastructure.Features.Cache
{
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

        public Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.SetAsync(cacheKey, value, expiration, cancellationToken);
        }

        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.RemoveAsync(key, cancellationToken);
        }
    }
    public class CacheServiceBridge<TValue> : ICacheService<TValue>
    {
        private readonly ICacheService<TValue> _innerCacheService;

        public CacheServiceBridge(CacheServiceFactory factory)
        {
            _innerCacheService = factory.GetCacheService<TValue>();
        }

        public Task<TValue?> GetAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.GetAsync(cacheKey, cancellationToken);
        }

        public Task<bool> SetAsync(string cacheKey, TValue value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.SetAsync(cacheKey, value, expiration, cancellationToken);
        }

        public Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default)
        {
            return _innerCacheService.RemoveAsync(cacheKey, cancellationToken);
        }
    }
}
