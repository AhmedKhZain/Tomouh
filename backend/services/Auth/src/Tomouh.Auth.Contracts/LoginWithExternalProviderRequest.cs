using Tomouh.Auth.Application.Commands.AddExternalLogin;
using Tomouh.Auth.Application.Commands.RegisterWithExternalProvider;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Application.Queries.LoginWithExternalProvider;

namespace Tomouh.Auth.Contracts;

public record RegisterWithExternalProviderRequest(AuthProvider Provider, string Token)
{
    public RegisterWithExternalProviderCommand ToCommand(Guid idempotencyKey)
        => new(Provider, Token, idempotencyKey);
}

public record LoginWithExternalProviderRequest(AuthProvider Provider, string Token)
{
    public LoginWithExternalProviderQuery ToQuery(Guid idempotencyKey)
        => new(Provider, Token, idempotencyKey);
}

public record AddExternalLoginRequest(AuthProvider Provider, string Token)
{
    public AddExternalLoginCommand ToCommand(Guid idempotencyKey)
        => new(Provider, Token, idempotencyKey);
}
