using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Contracts.Responses;

public class TFANeededAuthenticationResult : AuthenticationResult
{
    public string EmailPrefix { get; init; } = string.Empty;

    public TFANeededAuthenticationResult(User user, string? message = null)
        : base(user, message)
    {
        EmailPrefix = user.MainEmail.Email[0..3];
    }

    protected TFANeededAuthenticationResult() { }
}
