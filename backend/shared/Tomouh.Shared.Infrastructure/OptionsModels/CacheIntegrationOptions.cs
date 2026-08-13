namespace Tomouh.Shared.Infrastructure.OptionsModels;

/// <summary>
/// Provides configuration options for Redis cache integration.
/// <para><strong>Example configuration:</strong></para>
/// <code>
///  "RedisOptions": {
///    "Host": "localhost",
///    "Port": 6379,
///    "Password": "",
///    "UseSsl": false,
///    "AbortOnConnectFail": false,
///    "ConnectTimeout": 5000,
///    "SyncTimeout": 5000,
///    "InstanceName": "Tomouh_Auth_"
///  }
/// </code>
/// </summary>

/// <summary>
/// Configuration options for configuring Redis Cache Integration.
/// </summary>
public class CacheIntegrationOptions
{
    /// <summary>
    /// Gets or sets the primary Redis host/domain name or IP address.
    /// <para>Default is <c>"localhost"</c>.</para>
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Gets or sets the Redis server port.
    /// <para>Default is <c>6379</c>.</para>
    /// </summary>
    public int Port { get; set; } = 6379;

    /// <summary>
    /// Gets or sets the password used to authenticate with the Redis server.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SSL/TLS encryption should be used for the Redis connection.
    /// <para>Default is <c>false</c>.</para>
    /// </summary>
    public bool UseSsl { get; set; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether to allow connect failure (if false, connection attempts fail immediately).
    /// <para>Default is <c>false</c>.</para>
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;

    /// <summary>
    /// Gets or sets the connection timeout in milliseconds.
    /// <para>Default is <c>5000</c> ms (5 seconds).</para>
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets the sync operation timeout in milliseconds.
    /// <para>Default is <c>5000</c> ms (5 seconds).</para>
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;

    /// <summary>
    /// Gets or sets an optional prefix for all cache keys to isolate keys between microservices.
    /// <para>Example: <c>"Tomouh_Auth_"</c></para>
    /// </summary>
    public string? InstanceName { get; set; } = "Tomouh_";

    /// <summary>
    /// Builds the fully formatted StackExchange.Redis connection string based on configured properties.
    /// </summary>
    /// <returns>A formatted Redis connection string.</returns>
    public string ToConnectionString()
    {
        var builder = new List<string>
        {
            $"{Host}:{Port}",
            $"ssl={UseSsl}",
            $"abortConnect={AbortOnConnectFail}",
            $"connectTimeout={ConnectTimeout}",
            $"syncTimeout={SyncTimeout}"
        };

        if (!string.IsNullOrWhiteSpace(Password))
        {
            builder.Add($"password={Password}");
        }

        return string.Join(",", builder);
    }
}