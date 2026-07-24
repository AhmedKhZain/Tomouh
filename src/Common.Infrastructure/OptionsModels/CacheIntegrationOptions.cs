namespace Common.Infrastructure.OptionsModels;

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
    /// Gets or sets the domain or IP address of the Redis server.
    /// </summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port number of the Redis server.
    /// </summary>
    public string Port { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the password (often referred to as username in legacy Redis configs) 
    /// used to authenticate with the Redis server.
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether to use SSL/TLS for the connection.
    /// </summary>
    public bool UseSSL { get; set; }

    /// <summary>
    /// Gets the formatted connection string required by the Redis client.
    /// </summary>
    public string RedisConnectionString => $"{Domain}:{Port},password={UserName},ssl={UseSSL}";
}