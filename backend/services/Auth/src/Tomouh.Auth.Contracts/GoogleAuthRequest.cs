using Tomouh.Auth.Application.Commands.RegisterWithExternalProvider;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Application.Queries.LoginWithExternalProvider;

namespace Tomouh.Auth.Contracts;

public class ExternalAuthRequest
{
    public AuthProvider Provider { get; set; }
    public string Token { get; set; } = string.Empty;

    public LoginWithExternalProviderQuery ToLoginQuery(Guid requestId)
    {
        return new LoginWithExternalProviderQuery(Provider, Token, requestId);
    }

    public RegisterWithExternalProviderCommand ToRegisterCommand(Guid requestId)
    {
        return new RegisterWithExternalProviderCommand(Provider, Token, requestId);
    }
}