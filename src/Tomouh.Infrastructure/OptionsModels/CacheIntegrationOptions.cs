namespace Tomouh.Infrastructure.OptionsModels;

/// <summary>
/// Defines the available cache implementation strategies.
/// </summary>
public enum TargetedCacheType
{
    /// <summary>
    /// Uses the local application memory for caching. 
    /// Best for single-instance applications or development environments.
    /// </summary>
    InMemory,

    /// <summary>
    /// Uses a remote Redis server for distributed caching.
    /// Best for scalable, multi-instance production environments.
    /// </summary>
    Redis
}

/// <summary>
/// Provides configuration options for Redis cache integration.
/// <para><strong>Example configuration:</strong></para>
/// <code>
/// "CacheSettings": {
///   "CacheType": "Redis",
///   "RedisOptions": {
///     "Domain": "localhost",
///     "Port": "6379",
///     "UserName": "your_password",
///     "UseSSL": false
///   }
/// }
/// </code>
/// </summary>
public class CacheIntegrationOptions
{
    /// <summary>
    /// Gets or sets the desired caching strategy (InMemory or Redis).
    /// </summary>
    public TargetedCacheType CacheType { get; set; } = TargetedCacheType.InMemory;

    /// <summary>
    /// Gets or sets the domain or IP address of the Redis server.
    /// </summary>
    public string? Domain { get; set; }

    /// <summary>
    /// Gets or sets the port number of the Redis server.
    /// </summary>
    public string? Port { get; set; }

    /// <summary>
    /// Gets or sets the password/username used to authenticate with Redis.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS.
    /// </summary>
    public bool UseSSL { get; set; }

    /// <summary>
    /// Gets the formatted connection string required by the Redis client.
    /// </summary>
    public string RedisConnectionString => $"{Domain}:{Port},password={UserName},ssl={UseSSL}";
}