using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Application.Interfaces;

public interface IExternalAuthProviderFactory
{
    IExternalAuthProviderStrategy GetStrategy(AuthProvider provider);
}