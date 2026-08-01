using Common.AuditLogs;
using Common.BaseTypes;
using Common.Markups;
using MediatR;
using System.Text.Json;

namespace Tomouh.Application.Common.Models;

/// <summary>
/// Represents the Outbox pattern entity used to guarantee reliable event handling and dispatching.
/// </summary>
public class EventOutbox : IHasId<Guid>, IMultyWayCreatableTrackable
{
    private static readonly int _maxRetryAttempts = 10;
    /// <summary>
    /// Gets the unique identifier for the outbox entry.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the fully qualified assembly name of the event type for deserialization.
    /// </summary>
    public string EventTypeName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the outbox event category type.
    /// </summary>
    public EventOutboxType EventType { get; private set; }

    /// <summary>
    /// Gets a value indicating whether the event has been successfully handled.
    /// </summary>
    public bool IsHandled { get; private set; } = false;

    /// <summary>
    /// Gets the UTC timestamp when the event was created.
    /// </summary>
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets the UTC timestamp when the event was handled. Null if not yet handled.
    /// </summary>
    public DateTime? HandledAt { get; private set; } = null;

    /// <summary>
    /// Gets the list of error messages logged during processing retries.
    /// </summary>
    public List<string> ErrorMessage { get; private set; } = new List<string>();

    /// <summary>
    /// Gets the number of failed execution attempts.
    /// </summary>
    public int RetryCount { get; private set; } = 0;

    /// <summary>
    /// Gets the serialized JSON payload of the event notification.
    /// </summary>
    public string Notification { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the identifier of the user who initiated the event, if applicable.
    /// </summary>
    public Guid? CreatedBy { get; private set; }

    /// <summary>
    /// Gets the creation actor type (User or System).
    /// </summary>
    public CreationActorType ActorType { get; private set; } = CreationActorType.User;

    /// <summary>
    /// Gets a value indicating whether the maximum retry threshold should be bypassed.
    /// </summary>
    public bool SkipMaxRetryAttempts { get; private set; } = false;

    /// <summary>
    /// Gets the number of additional retry attempts permitted above the default threshold.
    /// </summary>
    public int SkipMoreCount { get; private set; } = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="EventOutbox"/> class for ORM / EF Core usage.
    /// </summary>
    public EventOutbox() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="EventOutbox"/> class with a specified notification.
    /// </summary>
    /// <param name="notification">The MediatR notification payload.</param>
    /// <param name="createBy">The optional unique identifier of the user creating the event.</param>
    public EventOutbox(INotification notification, Guid? createBy = null)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Id = Guid.NewGuid();
        var type = notification.GetType();

        EventTypeName = type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
        EventType = type.GetInterfaces().Any(a => a.Name == nameof(IDomainEvent))
            ? EventOutboxType.DomainEvent
            : EventOutboxType.IntegrationEvent;

        Notification = JsonSerializer.Serialize(notification, type);
        CreatedAt = DateTime.UtcNow;
        HandledAt = null;

        SetCreator(createBy);
    }

    /// <summary>
    /// Marks the outbox event as successfully handled and updates the completion timestamp.
    /// </summary>
    public void MarkAsHandled()
    {
        IsHandled = true;
        HandledAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a failure attempt by appending the error message and incrementing the retry count.
    /// </summary>
    /// <param name="message">An optional error message describing the failure details.</param>
    public void MarkAsFailed(string? message = null)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            ErrorMessage.Add(message);
        }

        RetryCount++;
    }

    /// <summary>
    /// Indicates whether the event can be retried based on configured limits.
    /// </summary>
    public bool CanRetry => RetryCount < (_maxRetryAttempts + SkipMoreCount);

    /// <summary>
    /// Sets the creator identity and determines whether the actor is a User or System.
    /// </summary>
    /// <param name="creatorId">The unique identifier of the creator. Null assigns System actor type.</param>
    public void SetCreator(Guid? creatorId = null)
    {
        if (creatorId == null || creatorId == Guid.Empty)
        {
            CreatedBy = null;
            ActorType = CreationActorType.System;
            return;
        }

        CreatedBy = creatorId;
        ActorType = CreationActorType.User;
    }
}
/// <summary>
/// Defines the category of the event stored in the outbox.
/// </summary>
public enum EventOutboxType
{
    /// <summary>
    /// Indicates an event scoped within the same bounded context.
    /// </summary>
    DomainEvent = 1,

    /// <summary>
    /// Indicates an event intended for external systems or microservices.
    /// </summary>
    IntegrationEvent = 2
}
