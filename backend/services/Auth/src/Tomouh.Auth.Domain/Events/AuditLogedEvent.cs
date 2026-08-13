using Tomouh.Shared.Kernel.AuditLogs;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.Events;

public record AuditLogedEvent(AuditLog Audit) : IIntegrationEvent;
