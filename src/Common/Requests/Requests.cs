using MediatR;

namespace Common.Requests;

public interface ICommand<out TResponse> : IRequest<TResponse>;
public interface IQuery<out TResponse> : IRequest<TResponse>;

public interface IValidateableRequest;
public interface IEventsIncludedRequest;
public interface IPerUserAuthraizedRequest;

public interface ICacheableRequest
{
    string PrefixCacheKey { get; }
    TimeSpan? Expiration { get; }
}

public interface IIdempotentRequest
{
    Guid RequestId { get; }
}
