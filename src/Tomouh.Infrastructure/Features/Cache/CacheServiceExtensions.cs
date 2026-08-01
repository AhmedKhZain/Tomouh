using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Cache;

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
            TargetedCacheType? overrideCacheType = null,
            string sectionName = "CacheSettings")
    {
        services.AddMemoryCache();

        var cacheOptions = configuration.GetSection(sectionName).Get<CacheIntegrationOptions>()
                           ?? new CacheIntegrationOptions();

        var finalCacheType = overrideCacheType ?? cacheOptions.CacheType;

        services.AddTransient(typeof(InMemoryCacheStrategy<>));
        services.AddTransient(typeof(RedisCacheStrategy<>));
        services.AddTransient<InMemoryCacheStrategy>();
        services.AddTransient<RedisCacheStrategy>();

        if (finalCacheType == TargetedCacheType.Redis)
        {
            if (string.IsNullOrWhiteSpace(cacheOptions.Domain) || string.IsNullOrWhiteSpace(cacheOptions.Port))
            {
                throw new InvalidOperationException("Redis configuration (Domain and Port) must be provided when CacheType is set to Redis.");
            }

            services.AddSingleton(cacheOptions);

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = cacheOptions.RedisConnectionString;
                options.InstanceName = "";
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
