using Tomouh.Auth.Application.Commands.ForgotPassword;

namespace Tomouh.Auth.Contracts;

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;

    public ForgotPasswordCommand ToCommand(Guid idempotencyKey) => new ForgotPasswordCommand(Email, idempotencyKey);
}
