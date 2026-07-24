using Common.BaseTypes;
using Common.Markups;

namespace Common.AuditLogs;

public interface IAudit<OldValType> : IHasId<Guid>, IMultyWayCreatableTrackable
{
    string EntityName { get; }
    string EntityId { get; }
    AuditActionType Action { get; }
    OldValType? OldValues { get; }
    DateTime From { get; }
    DateTime To { get; }
    bool IsRecovered { get; }
    DateTime? RecoveredAt { get; }
    Guid? RecoveredByUserId { get; }


}
