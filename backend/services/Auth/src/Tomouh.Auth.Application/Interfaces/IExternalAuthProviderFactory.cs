using Tomouh.Auth.Application.Common;

namespace Tomouh.Auth.Application.Interfaces;

public interface IExternalAuthProviderFactory
{
    IExternalAuthProviderStrategy GetStrategy(AuthProvider provider);
}