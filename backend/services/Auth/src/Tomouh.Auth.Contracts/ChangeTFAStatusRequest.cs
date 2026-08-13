using Tomouh.Auth.Application.Commands.ChangeTFAStatus;

namespace Tomouh.Auth.Contracts;

public class ChangeTFAStatusRequest
{
    public bool IsEnabled { get; set; }
    public string Password { get; set; } = string.Empty;

    public ChangeTFAStatusCommand ToCommand(Guid idempotencyKey)
        => new(IsEnabled, Password, idempotencyKey);
}
