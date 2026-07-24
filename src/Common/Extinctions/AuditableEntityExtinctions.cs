using Common.AuditLogs;
using Common.BaseTypes;

namespace Common.Extinctions;

public static class AuditableEntityExtinctions
{
    public static AuditLog CreateUpdateAudit(this IAuditable auditable, string mainAggregateName = null)
    {
        return AuditLog.Create(auditable, AuditActionType.Update, mainAggregateName);
    }
    public static AuditLog CreateCreationAudit(this IAuditable auditable, string mainAggregateName = null)
    {
        return AuditLog.Create(auditable, AuditActionType.Create, mainAggregateName);
    }
    public static AuditLog CreateDeletionAudit(this IAuditable auditable, string mainAggregateName = null)
    {
        return AuditLog.Create(auditable, AuditActionType.Delete, mainAggregateName);
    }
}
