using Common.BaseTypes;

namespace Tomouh.Domain.Auth.Events;

public record UserTokenCreatedEvent(
    Guid UserId,
    string UserEmail,
    string ShowName,
    string PlainToken,
    TokenType TokenType,
    DateTime ExpiresAt
) : IIntegrationEvent;