using Common.BaseTypes;
using Common.DataConverters;

namespace Common.AuditLogs;

/// <summary>
/// Represents an audit log record capturing the point-in-time state changes of a system entity.
/// Supports both full-state snapshots and field-level delta dictionary tracking.
/// </summary>
public class AuditLog : IAudit<string>
{
    public Guid Id { get; private set; }
    public string EntityName { get; private set; } = default!;
    public string EntityId { get; private set; } = default!;
    public AuditActionType Action { get; private set; } = default!;
    public string? OldValues { get; private set; }
    public DateTime From { get; private set; }
    public DateTime To { get; private set; }
    public bool IsRecovered { get; private set; } = false;
    public DateTime? RecoveredAt { get; private set; }
    public Guid? RecoveredByUserId { get; private set; }
    public CreationActorType ActorType { get; private set; } = CreationActorType.User;
    public Guid? CreatedBy { get; private set; }

    /// <summary>
    /// Sets the unique identifier of the user or system component that triggered the audited action.
    /// </summary>
    /// <param name="creatorId">The unique identifier of the performing user or entity.</param>
    /// <param name="actorType">The category of actor performing the action. Defaults to <see cref="CreationActorType.User"/>.</param>
    public void SetCreator(Guid? creatorId, CreationActorType actorType = CreationActorType.User)
    {
        CreatedBy = creatorId;
        ActorType = actorType;
    }

    /// <summary>
    /// Records the administrative user performing a data recovery operation on this log entry.
    /// </summary>
    /// <param name="userId">The unique identifier of the administrator.</param>
    public void SetRecoveredUser(Guid userId)
    {
        RecoveredByUserId = userId;
    }

    private AuditLog()
    {
    }

    /// <summary>
    /// Creates a new audit log record from the original entity state prior to modification.
    /// </summary>
    /// <param name="originalState">The auditable state of the entity before the mutation.</param>
    /// <param name="action">The category of mutation performed on the entity. Defaults to <see cref="AuditActionType.Update"/>.</param>
    /// <param name="editedEntityName">Optional root context qualifier to prepend as a namespace scope to the entity name.</param>
    /// <param name="customEntityId">Optional alternative tracking identifier if the internal ID structure deviates from standard conventions.</param>
    /// <returns>A fully initialized and populated <see cref="AuditLog"/> instance.</returns>
    public static AuditLog Create(
        IAuditable originalState,
        AuditActionType action = AuditActionType.Update,
        string? editedEntityName = null,
        string? customEntityId = null,
        bool usedBySystem = false,
        Guid? createdBy = null)
    {
        string entityName = string.IsNullOrWhiteSpace(editedEntityName)
            ? originalState.GetType().Name
            : $"{editedEntityName}.{originalState.GetType().Name}";

        var oldValues = (action == AuditActionType.Create)
                ? null
                : originalState.Serialize();

        var audit = new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityName = entityName,
            EntityId = customEntityId ?? originalState.Id?.ToString(),
            Action = action,
            OldValues = oldValues,
            From = originalState.LastUpdate ?? originalState.CreatedAt,
            To = DateTime.UtcNow,
            IsRecovered = false,
            RecoveredAt = null,
            RecoveredByUserId = null,
            ActorType = usedBySystem ? CreationActorType.System : CreationActorType.User,
            CreatedBy = createdBy,
        };

        return audit;
    }

}
