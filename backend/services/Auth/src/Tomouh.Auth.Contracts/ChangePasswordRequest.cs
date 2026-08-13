using Tomouh.Auth.Application.Commands.ChangePassword;

namespace Tomouh.Auth.Contracts;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;

    public ChangePasswordCommand ToCommand(Guid idempotencyKey)
        => new(CurrentPassword, NewPassword, idempotencyKey);
}
