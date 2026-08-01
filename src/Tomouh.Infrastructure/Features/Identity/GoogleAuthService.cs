using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Tomouh.Application.Auth.Commands.RegisterWithGoogle;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Identity;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly GoogleAuthData _googleAuthData;

    public GoogleAuthService(IOptions<GoogleAuthData> options)
    {
        _googleAuthData = options.Value;
    }

    public async Task<GoogleAuthPayload> ValidateTokenAsync(string googleToken, CancellationToken cancellationToken = default)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _googleAuthData.GoogleClientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken, settings);

        return new GoogleAuthPayload(
            SubjectId: payload.Subject,
            Email: payload.Email,
            FirstName: payload.GivenName ?? payload.Name ?? $"User{Guid.NewGuid()}",
            LastName: payload.FamilyName ?? string.Empty,
            Name: payload.Name ?? payload.Email,
            PictureUrl: payload.Picture
        );
    }
}
