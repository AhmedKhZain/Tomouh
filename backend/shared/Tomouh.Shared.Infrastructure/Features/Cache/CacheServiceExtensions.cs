using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Tomouh.Shared.Infrastructure.OptionsModels;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Shared.Infrastructure.Features.Cache;

public static class CacheServiceExtensions
{
    /// <summary>
    /// Registers Redis distributed caching services and locking mechanisms using <see cref="CacheIntegrationOptions"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The application <see cref="IConfiguration"/> instance.</param>
    /// <param name="sectionName">The configuration section name containing Redis settings. Defaults to <c>"RedisOptions"</c>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> for chaining.</returns>
    /// <example>
    /// <code>
    /// // In appsettings.json:
    /// {
    ///   "RedisOptions": {
    ///     "Host": "127.0.0.1",
    ///     "Port": 6379,
    ///     "Password": "SecretPassword123",
    ///     "UseSsl": false,
    ///     "AbortOnConnectFail": false,
    ///     "ConnectTimeout": 5000,
    ///     "SyncTimeout": 5000,
    ///     "InstanceName": "Tomouh_Auth_"
    ///   }
    /// }
    ///
    /// // In Program.cs:
    /// builder.Services.AddCaching(builder.Configuration);
    /// </code>
    /// </example>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "RedisOptions")
    {
        // 1. Bind and validate options from appsettings.json
        var cacheOptions = configuration.GetSection(sectionName).Get<CacheIntegrationOptions>()
                           ?? new CacheIntegrationOptions();

        var connectionString = cacheOptions.ToConnectionString();

        // 2. Register StackExchange.Redis Distributed Cache
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = connectionString;
            options.InstanceName = cacheOptions.InstanceName;
        });

        // 3. Register Native IConnectionMultiplexer for Atomic Distributed Locks
        services.AddSingleton<IConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));

        // 4. Register Unified Cache Service Strategy
        services.AddSingleton<ICacheService, RedisCacheStrategy>();

        return services;
    }
}