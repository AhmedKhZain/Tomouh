using System.Text.Json.Serialization;

namespace Common.BaseTypes;

/// <summary>
/// Defines a contract for aggregate roots, managing domain and integration events.
/// </summary>
public interface IAggregate
{
    [JsonIgnore]
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    /// <summary>
    /// Returns all domain events and clears the internal list.
    /// </summary>
    IReadOnlyList<IDomainEvent> DequeueDomainEvents();

    [JsonIgnore]
    IReadOnlyList<IIntegrationEvent> IntegrationEvents { get; }
    /// <summary>
    /// Returns all integration events and clears the internal list.
    /// </summary>
    IReadOnlyList<IIntegrationEvent> DequeueIntegrationEvents();

    void AddDomainEvent(IDomainEvent domainEvent);
    void AddIntegrationEvent(IIntegrationEvent integrationEvent);
    void ClearDomainEvents();
    void ClearIntegrationEvents();
}