using Tomouh.Auth.Application.Common;

namespace Tomouh.Auth.Application.Interfaces;

public interface IExternalAuthProviderStrategy
{
    AuthProvider Provider { get; }
    Task<ExternalAuthPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
