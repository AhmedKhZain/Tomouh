using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Infrastructure.Options;

namespace Tomouh.Auth.Infrastructure.ExternalAuth;

public class GoogleAuthProviderStrategy : IExternalAuthProviderStrategy
{
    private readonly ProviderSettings _googleSettings;

    public GoogleAuthProviderStrategy(IOptions<ExternalAuthSettings> externalAuthOptions)
    {
        _googleSettings = externalAuthOptions.Value.Google;
    }

    public AuthProvider Provider => AuthProvider.Google;

    public async Task<ExternalAuthPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var validationSettings = new GoogleJsonWebSignature.ValidationSettings();

        if (!string.IsNullOrWhiteSpace(_googleSettings.ClientId))
        {
            validationSettings.Audience = new[] { _googleSettings.ClientId };
        }

        var payload = await GoogleJsonWebSignature.ValidateAsync(token, validationSettings);

        return new ExternalAuthPayload(
            SubjectId: payload.Subject,
            Email: payload.Email,
            FirstName: payload.GivenName ?? string.Empty,
            LastName: payload.FamilyName ?? string.Empty,
            PictureUrl: payload.Picture,
            Name: payload.Name
        );
    }
}