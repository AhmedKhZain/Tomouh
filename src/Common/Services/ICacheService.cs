namespace Common.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface ICacheService<TValue>
{
    Task<TValue?> GetAsync(string cacheKey);
    Task SetAsync<TValue>(string cacheKey, TValue? value, TimeSpan? expiration);
}
