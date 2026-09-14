using Microsoft.Extensions.DependencyInjection;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Application.Interfaces;

namespace Tomouh.Auth.Infrastructure.ExternalAuth;

public class ExternalAuthProviderFactory(IServiceProvider serviceProvider) : IExternalAuthProviderFactory
{
    public IExternalAuthProviderStrategy GetStrategy(AuthProvider provider)
    {
        var strategy = serviceProvider.GetKeyedService<IExternalAuthProviderStrategy>(provider);
        return strategy ?? throw new NotSupportedException($"Authentication provider '{provider}' is not supported.");
    }
}