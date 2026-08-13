using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Shared.Kernel.AuditLogs;

public static class AuditErrors
{
    public static readonly Error NullAuditableEntity = Error.Conflict(
        code: "AuditLog.NullAuditableEntity",
        description: "Cannot generate audit log for a null entity reference.");
}