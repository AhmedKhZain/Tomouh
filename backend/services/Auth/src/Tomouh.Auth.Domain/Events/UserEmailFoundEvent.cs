using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.Events;

public record UserEmailFoundEvent(User User) : IDomainEvent;


