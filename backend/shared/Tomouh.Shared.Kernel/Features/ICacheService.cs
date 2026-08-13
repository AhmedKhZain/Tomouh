namespace Tomouh.Shared.Kernel.Features;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string cacheKey, CancellationToken cancellationToken = default);
    Task<bool> SetAsync<T>(string cacheKey, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default);
    /// <summary>
    /// Tries to acquire a distributed lock for the specified key.
    /// </summary>
    /// <param name="key">The resource lock key.</param>
    /// <param name="expiry">Lock expiration to prevent deadlocks.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A lock handle instance if acquired successfully; otherwise, null.</returns>
    Task<IAsyncDisposable?> AcquireLockAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}
