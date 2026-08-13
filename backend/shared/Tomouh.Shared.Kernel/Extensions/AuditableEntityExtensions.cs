using Tomouh.Shared.Kernel.AuditLogs;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Shared.Kernel.Extensions;

public static class AuditableEntityExtensions
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
