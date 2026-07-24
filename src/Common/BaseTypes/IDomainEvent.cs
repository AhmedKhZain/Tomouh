using MediatR;

namespace Common.BaseTypes;

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent : INotification;

/// <summary>
/// Marker interface for integration events.
/// </summary>
public interface IIntegrationEvent : INotification;