using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

public abstract partial class MongoBaseContext
{
    /// <summary>
    /// Determines whether any tracked entities contain pending domain or integration events.
    /// </summary>
    public bool AnyEvents()
    {
        return TrackedEntities.Any(a =>
            (a.DomainEvents != null && a.DomainEvents.Count > 0) ||
            (a.IntegrationEvents != null && a.IntegrationEvents.Count > 0));
    }

    /// <summary>
    /// Collects and dequeues all pending domain events from tracked entities.
    /// </summary>
    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var domainEvents = new List<IDomainEvent>();

        foreach (var aggregate in TrackedEntities)
        {
            var events = aggregate.DequeueDomainEvents();
            if (events is { Count: > 0 })
            {
                domainEvents.AddRange(events);
            }
        }

        return domainEvents.AsReadOnly();
    }

    /// <summary>
    /// Collects and dequeues all pending integration events from tracked entities.
    /// </summary>
    public IReadOnlyList<IIntegrationEvent> CollectIntegrationEvents()
    {
        var integrationEvents = new List<IIntegrationEvent>();

        foreach (var aggregate in TrackedEntities)
        {
            var events = aggregate.DequeueIntegrationEvents();
            if (events is { Count: > 0 })
            {
                integrationEvents.AddRange(events);
            }
        }

        return integrationEvents.AsReadOnly();
    }
}