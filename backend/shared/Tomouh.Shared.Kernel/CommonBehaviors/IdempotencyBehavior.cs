using MediatR;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Shared.Kernel.CommonBehaviors;

/// <summary>
/// Pipeline behavior that guarantees idempotent execution of requests using distributed locks and cached results.
/// </summary>
public class IdempotencyBehavior<TRequest, TResponse, TValue> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IIdempotentRequest
    where TResponse : IResultOf<TValue>
{
    private readonly ICacheService _cacheService;
    private readonly string _prefix = "Idempotency:";
    private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(15);
    private readonly TimeSpan _lockTimeout = TimeSpan.FromSeconds(30);

    public IdempotencyBehavior(ICacheService cacheService)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var cacheKey = $"{_prefix}{request.RequestId}";

        var cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
        if (cachedValue is not null)
        {
            return (TResponse)ResultOf<TValue>.Cached(cachedValue);
        }

        var lockKey = $"lock:{cacheKey}";
        await using var lockHandle = await _cacheService.AcquireLockAsync(lockKey, _lockTimeout, cancellationToken);

        if (lockHandle is null)
        {
            await Task.Delay(500, cancellationToken);
            cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
            if (cachedValue is not null)
            {
                return (TResponse)ResultOf<TValue>.Cached(cachedValue);
            }

            throw new InvalidOperationException($"A request with RequestId '{request.RequestId}' is currently being processed.");
        }

        cachedValue = await _cacheService.GetAsync<TValue>(cacheKey, cancellationToken);
        if (cachedValue is not null)
        {
            return (TResponse)ResultOf<TValue>.Cached(cachedValue);
        }

        var response = await next();

        if (!response.IsFailure && response.Value is not null)
        {
            await _cacheService.SetAsync(cacheKey, response.Value, _cacheDuration, cancellationToken);
        }

        return response;
    }
}