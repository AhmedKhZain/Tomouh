using Common.AuditLogs;
using Common.BaseTypes;

namespace Tomouh.Domain;

public record AuditLogedEvent(AuditLog audit) : IIntegrationEvent;
