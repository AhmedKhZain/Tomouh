using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace Common.BaseTypes;

/// <summary>
/// Base class for auditable Aggregate Roots in Domain-Driven Design (DDD).
/// Inherits from <see cref="AuditableEntity{TId}"/> to enforce timestamp mutations and historical state capture tracking.
/// Manages domain and integration events collection.
/// </summary>
/// <typeparam name="TId">The type of the entity's unique identifier.</typeparam>
public abstract class AuditableAggregateRoot<TId> : AuditableEntity<TId>, IAggregate
{
    [JsonIgnore]
    [BsonIgnore]
    private readonly List<IDomainEvent> _domainEvents = new();
    [JsonIgnore]
    [BsonIgnore]
    private readonly List<IIntegrationEvent> _integrationEvents = new();

    protected AuditableAggregateRoot() : base() { }

    protected AuditableAggregateRoot(Guid? createdBy = null) : base(createdBy) { }

    protected AuditableAggregateRoot(TId id, Guid? createdBy = null) : base(id, createdBy) { }

    /// <summary>
    /// Gets a read-only collection of accumulated domain events.
    /// </summary>
    [BsonIgnore]
    [JsonIgnore]
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Gets a read-only collection of accumulated integration events.
    /// </summary>
    [BsonIgnore]
    [JsonIgnore]
    public IReadOnlyList<IIntegrationEvent> IntegrationEvents => _integrationEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the internal collection for late dispatching.
    /// </summary>
    public void AddDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>
    /// Adds an integration event to the internal collection for outbox or message broker processing.
    /// </summary>
    public void AddIntegrationEvent(IIntegrationEvent integrationEvent) => _integrationEvents.Add(integrationEvent);

    /// <summary>
    /// Clears the domain events collection.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();

    /// <summary>
    /// Clears the integration events collection.
    /// </summary>
    public void ClearIntegrationEvents() => _integrationEvents.Clear();

    /// <summary>
    /// Extracts and returns all domain events, then flushes the internal collection.
    /// </summary>
    public IReadOnlyList<IDomainEvent> DequeueDomainEvents()
    {
        var events = _domainEvents.ToList();
        _domainEvents.Clear();
        return events;
    }

    /// <summary>
    /// Extracts and returns all integration events, then flushes the internal collection.
    /// </summary>
    public IReadOnlyList<IIntegrationEvent> DequeueIntegrationEvents()
    {
        var events = _integrationEvents.ToList();
        _integrationEvents.Clear();
        return events;
    }

}
