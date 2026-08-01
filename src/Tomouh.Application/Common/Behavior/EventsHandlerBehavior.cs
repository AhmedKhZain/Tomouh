using Common.BaseTypes;
using Common.Models;
using Common.Requests;
using Common.ResultOf;
using MediatR;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Application.Common.Interfaces.Repositories;
using Tomouh.Application.Common.Models;

namespace Tomouh.Application.Common.Behavior;

public class EventsHandlerBehavior<TRequest, TResponse>(
    IDomainEventCollector _eventCollector,
    INotificationLogRepository _notificationLogRepository,
    CurrentUser _currentUser,
    IPublisher _publisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IEventsIncludedRequest
    where TResponse : IResultOf
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var response = await next();

        var integrationEvents = new List<IIntegrationEvent>();

        integrationEvents.AddRange(_eventCollector.DequeueIntegrationEvents());

        while (_eventCollector.AnyDomainEvents || _eventCollector.AnyIntgrationEvents)
        {
            foreach (var domainEvent in _eventCollector.DequeueDomainEvents())
            {
                await _publisher.Publish(domainEvent, cancellationToken);
            }

            integrationEvents.AddRange(_eventCollector.DequeueIntegrationEvents().AsEnumerable());
        }

        if (integrationEvents.Count > 0)
        {
            var outboxEvents = integrationEvents
                .Select(e => new EventOutbox(e, _currentUser.Id))
                .ToList();

            await _notificationLogRepository.InsertAsync(outboxEvents, cancellationToken);
        }

        return response;
    }
}
