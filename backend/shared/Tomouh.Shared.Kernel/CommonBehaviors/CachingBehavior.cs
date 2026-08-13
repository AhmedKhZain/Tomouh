using MediatR;
using System.Reflection;
using System.Text;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Shared.Kernel.CommonBehaviors;

/// <summary>
/// Pipeline behavior responsible for intercepting <see cref="ICacheableRequest"/> requests, 
/// serving cached responses when available, or executing and caching successful results.
/// Uses distributed locks to prevent Cache Stampede (Thundering Herd) on cache miss.
/// </summary>
public class CachingBehavior<TRequest, TResponse, TValue> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICacheableRequest
    where TResponse : ResultOf<TValue>
{
    private readonly ICacheService _cacheService;
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(10);

    public CachingBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        string cacheKey = GenerateCacheKey(request);

        // 1. Try fast path read from cache
        TValue? cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
        if (cachedValue is not null)
        {
            return (TResponse)ResultOf<TValue>.Cached(cachedValue);
        }

        string lockKey = $"lock:{cacheKey}";
        await using var lockHandle = await _cacheService.AcquireLockAsync(lockKey, _lockTimeout, cancellationToken);

        if (lockHandle is not null)
        {
            cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
            if (cachedValue is not null)
            {
                return (TResponse)ResultOf<TValue>.Cached(cachedValue);
            }

            var response = await next();

            if (!response.IsFailure && response.Value is not null)
            {
                var timeToLive = request.Expiration ?? _defaultCacheDuration;
                await _cacheService.SetAsync(cacheKey, response.Value, timeToLive, cancellationToken);
            }

            return response;
        }

        await Task.Delay(500, cancellationToken);
        cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
        if (cachedValue is not null)
        {
            return (TResponse)ResultOf<TValue>.Cached(cachedValue);
        }

        return await next();
    }

    private static string GenerateCacheKey(TRequest request)
    {
        var sb = new StringBuilder();
        var requestType = typeof(TRequest);

        sb.Append(request.PrefixCacheKey ?? requestType.Name);

        var propertiesWithAttribute = requestType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Select(p => new
            {
                Property = p,
                Attribute = p.GetCustomAttribute<CacheKeyParameterAttribute>()
            })
            .Where(x => x.Attribute != null)
            .OrderBy(x => x.Property.Name);

        foreach (var item in propertiesWithAttribute)
        {
            var value = item.Property.GetValue(request);
            if (value is not null)
            {
                string keyName = !string.IsNullOrWhiteSpace(item.Attribute!.KeyName)
                    ? item.Attribute.KeyName
                    : item.Property.Name;

                sb.Append($"_{keyName}:{value}");
            }
        }

        return sb.ToString();
    }
}