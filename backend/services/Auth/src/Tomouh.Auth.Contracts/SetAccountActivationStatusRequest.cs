using Tomouh.Auth.Application.Commands.SetAccountActivationStatus;

namespace Tomouh.Auth.Contracts;

public class SetAccountActivationStatusRequest
{
    public bool IsActive { get; set; }

    public SetAccountActivationStatusCommand ToCommand(Guid targetUserId, Guid idempotencyKey)
        => new(targetUserId, IsActive, idempotencyKey);
}
