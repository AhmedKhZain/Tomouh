using Tomouh.Application.Auth.Commands.RegisterWithGoogle;

namespace Tomouh.Application.Common.Interfaces;

public interface IGoogleAuthService
{
    Task<GoogleAuthPayload> ValidateTokenAsync(string googleToken, CancellationToken cancellationToken = default);
}