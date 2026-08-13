using Tomouh.Auth.Application.Commands.SetCommentingStatus;

namespace Tomouh.Auth.Contracts;

public class SetCommentingStatusRequest
{
    public bool IsDisabled { get; set; }

    public SetCommentingStatusCommand ToCommand(Guid targetUserId, Guid idempotencyKey)
        => new(targetUserId, IsDisabled, idempotencyKey);
}
