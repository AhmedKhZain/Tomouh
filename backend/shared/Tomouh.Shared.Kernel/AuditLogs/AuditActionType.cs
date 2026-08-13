using Tomouh.Shared.Kernel.DataAnnotation;

namespace Tomouh.Shared.Kernel.AuditLogs;

[StoreEnumAsString(maxLength: 20)]
public enum AuditActionType
{
    Create = 0,
    Update = 1,
    Delete = 2,
}
