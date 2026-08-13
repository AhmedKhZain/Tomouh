using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.Events;

public record UserTokenCreatedEvent(
    Guid UserId,
    string UserEmail,
    string ShowName,
    string PlainToken,
    TokenType TokenType,
    DateTime ExpiresAt
) : IIntegrationEvent;