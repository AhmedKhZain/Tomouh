using Common.AuditLogs;
using Common.BaseTypes;

namespace Tomouh.Domain.Common.Events;

public record AuditLogedEvent(AuditLog audit) : IIntegrationEvent;
