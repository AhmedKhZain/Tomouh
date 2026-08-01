namespace Common.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
}

public interface ICacheService<T>
{
    Task<T?> GetAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task<bool> SetAsync(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string cacheKey, CancellationToken cancellationToken = default);
}