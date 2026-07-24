using Common.Infrastructure.OptionsModels;
using Common.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Features.Cache;

public class CacheServiceFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TargetedCacheType _cacheType;

    public CacheServiceFactory(IServiceProvider serviceProvider, TargetedCacheType cacheType)
    {
        _serviceProvider = serviceProvider;
        _cacheType = cacheType;
    }

    public ICacheService<TValue> GetCacheService<TValue>()
    {
        return _cacheType switch
        {
            TargetedCacheType.InMemory => _serviceProvider.GetRequiredService<InMemoryCacheStrategy<TValue>>(),
            TargetedCacheType.Redis => _serviceProvider.GetRequiredService<RedisCacheStrategy<TValue>>(),
            _ => throw new NotImplementedException($"Cache type {_cacheType} is not supported.")
        };
    }

    public ICacheService GetCacheService()
    {
        return _cacheType switch
        {
            TargetedCacheType.InMemory => _serviceProvider.GetRequiredService<InMemoryCacheService>(),
            TargetedCacheType.Redis => _serviceProvider.GetRequiredService<RedisCacheService>(),
            _ => throw new NotImplementedException($"Cache type {_cacheType} is not supported.")
        };
    }
}
