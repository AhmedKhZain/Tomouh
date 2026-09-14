using MediatR;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.Outbox;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Shared.Kernel.CommonBehaviors;

public class EventsHandlerBehavior<TRequest, TResponse>(
    IUnitOfWork unitOfWork,
    INotificationLogRepository notificationLogRepository,
    CurrentUser currentUser,
    IPublisher publisher)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IEventsIncludedRequest
    where TResponse : IResultOf
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {

        try
        {
            //await unitOfWork.StartTransactionAsync(cancellationToken);

            var response = await next();

            if (response.IsFailure)
            {
                //await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return response;
            }

            var integrationEvents = new List<IIntegrationEvent>();

            integrationEvents.AddRange(unitOfWork.CollectIntegrationEvents());

            while (unitOfWork.AnyEvents())
            {
                var domainEvents = unitOfWork.CollectDomainEvents();

                foreach (var domainEvent in domainEvents)
                {
                    await publisher.Publish(domainEvent, cancellationToken);
                }

                integrationEvents.AddRange(unitOfWork.CollectIntegrationEvents());
            }

            if (integrationEvents.Count > 0)
            {
                var outboxEvents = integrationEvents
                    .Select(e => new EventOutbox(e, currentUser.Id))
                    .ToList();

                await notificationLogRepository.InsertAsync(outboxEvents, cancellationToken);
            }

            //await unitOfWork.CommitTransactionAsync(cancellationToken);

            return response;
        }
        catch
        {
            //await unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}