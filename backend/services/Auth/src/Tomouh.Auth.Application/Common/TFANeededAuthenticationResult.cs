using Tomouh.Auth.Domain.Entities;

namespace Tomouh.Auth.Application.Common;

public class TFANeededAuthenticationResult : AuthenticationResult
{
    public string EmailPrefix { get; init; }
    public TFANeededAuthenticationResult(User user, string? massege = null) : base(user, massege)
    {
        EmailPrefix = user.MainEmail.Email[0..3];
    }

}
