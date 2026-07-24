using Common.Infrastructure.OptionsModels;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Features.Cache;

public static class CacheServiceExtensions
{
    /// <summary>
    /// Registers caching services.
    /// <para><strong>Example configuration:</strong></para>
    /// <code>
    /// "CacheSettings": {
    ///   "CacheType": "Redis",
    ///   "RedisOptions": {
    ///     "Domain": "localhost",
    ///     "Port": "6379",
    ///     "UserName": "yourpassword",
    ///     "UseSSL": false
    ///   }
    /// }
    /// </code>
    /// </summary>
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration,
        TargetedCacheType? cacheType = null,
        string sectionName = "CacheSettings")
    {
        services.AddMemoryCache();

        var typeString = configuration[$"{sectionName}:CacheType"] ?? "InMemory";
        var finalCacheType = cacheType ?? Enum.Parse<TargetedCacheType>(typeString);

        services.AddTransient(typeof(InMemoryCacheStrategy<>));
        services.AddTransient(typeof(RedisCacheStrategy<>));
        services.AddTransient<InMemoryCacheService>();
        services.AddTransient<RedisCacheService>();

        if (finalCacheType == TargetedCacheType.Redis)
        {
            var redisOptions = configuration.GetSection($"{sectionName}:RedisOptions").Get<CacheIntegrationOptions>()
                ?? throw new InvalidOperationException("Redis configuration options are missing.");

            services.AddSingleton(redisOptions);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisOptions.RedisConnectionString;
                options.InstanceName = "Trainova_";
            });
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddSingleton(sp => new CacheServiceFactory(sp, finalCacheType));
        services.AddTransient(typeof(ICacheService<>), typeof(CacheServiceBridge<>));
        services.AddTransient<ICacheService, CacheServiceBridge>();

        return services;
    }
}
