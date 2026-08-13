using Tomouh.Auth.Application.Commands.SetBlockStatus;

namespace Tomouh.Auth.Contracts;

public class SetBlockStatusRequest
{
    public bool IsBlocked { get; set; }

    public SetBlockStatusCommand ToCommand(Guid targetUserId, Guid idempotencyKey)
        => new(targetUserId, IsBlocked, idempotencyKey);
}
