using Common.DataConvrters;

namespace Common.AuditLogs;

[StoreEnumAsString(maxLength: 20)]
public enum AuditActionType
{
    Create = 0,
    Update = 1,
    Delete = 2,
}
