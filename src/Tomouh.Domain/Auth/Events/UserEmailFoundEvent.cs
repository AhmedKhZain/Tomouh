using Common.BaseTypes;

namespace Tomouh.Domain.Auth.Events;

public record UserEmailFoundEvent(User User) : IDomainEvent;


