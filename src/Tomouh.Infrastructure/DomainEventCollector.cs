using Common.BaseTypes;
using Tomouh.Application.Common.Interfaces;

namespace Tomouh.Infrastructure;

public class DomainEventCollector : IDomainEventCollector
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<IIntegrationEvent> _integrationEvents = new();

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public IReadOnlyList<IIntegrationEvent> GetIntegrationEvents() => _integrationEvents.AsReadOnly();

    public void CollectEventsFromAggregate(IAggregate aggregate)
    {
        if (aggregate is null) return;

        // Extract domain events using your Dequeue method
        var domainEvents = aggregate.DequeueDomainEvents();
        if (domainEvents.Any())
        {
            _domainEvents.AddRange(domainEvents);
        }

        // Extract integration events using your Dequeue method
        var integrationEvents = aggregate.DequeueIntegrationEvents();
        if (integrationEvents.Any())
        {
            _integrationEvents.AddRange(integrationEvents);
        }
    }

    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }

    public IReadOnlyList<IIntegrationEvent> DequeueIntegrationEvents()
    {
        var events = _integrationEvents.ToList();
        _integrationEvents.Clear();
        return events;
    }
}