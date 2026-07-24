using Common.BaseTypes;

namespace Tomouh.Application.Common.Interfaces;

public interface IDomainEventCollector
{
    IReadOnlyList<IDomainEvent> GetDomainEvents();
    IReadOnlyList<IIntegrationEvent> GetIntegrationEvents();

    void CollectEventsFromAggregate(IAggregate aggregate);

    IReadOnlyList<IDomainEvent> DequeueDomainEvents();
    IReadOnlyList<IIntegrationEvent> DequeueIntegrationEvents();
}