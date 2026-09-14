using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Application.Interfaces;

public interface IExternalAuthProviderStrategy
{
    AuthProvider Provider { get; }
    Task<ExternalAuthPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
}
