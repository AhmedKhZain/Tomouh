using Tomouh.Domain.Auth;

namespace Tomouh.Application.Auth.Common;

public class TFANeededAuthenticationResult : AuthenticationResult
{
    public string EmailPrefix { get; init; }
    public TFANeededAuthenticationResult(User user, string? massege = null) : base(user, massege)
    {
        EmailPrefix = user.Email.Email[0..3];
    }

}
