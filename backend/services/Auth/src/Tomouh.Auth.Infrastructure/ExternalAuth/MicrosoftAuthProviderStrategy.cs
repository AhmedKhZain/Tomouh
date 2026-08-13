using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Tomouh.Auth.Application.Common;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Infrastructure.Options;

namespace Tomouh.Auth.Infrastructure.ExternalAuth;

public class MicrosoftAuthProviderStrategy : IExternalAuthProviderStrategy
{
    private readonly MicrosoftProviderSettings _microsoftSettings;
    private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

    public MicrosoftAuthProviderStrategy(IOptions<ExternalAuthSettings> externalAuthOptions)
    {
        _microsoftSettings = externalAuthOptions.Value.Microsoft;
        var tenant = string.IsNullOrWhiteSpace(_microsoftSettings.TenantId) ? "common" : _microsoftSettings.TenantId;

        _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            $"https://login.microsoftonline.com/{tenant}/v2.0/.well-known/openid-configuration",
            new OpenIdConnectConfigurationRetriever());
    }

    public AuthProvider Provider => AuthProvider.Microsoft;

    public async Task<ExternalAuthPayload> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        var openIdConfig = await _configurationManager.GetConfigurationAsync(cancellationToken);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = !string.IsNullOrWhiteSpace(_microsoftSettings.ClientId),
            ValidAudience = _microsoftSettings.ClientId,
            ValidateLifetime = true,
            IssuerSigningKeys = openIdConfig.SigningKeys
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, validationParameters, out _);

        var subjectId = principal.FindFirst("sub")?.Value
                        ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
                        ?? throw new InvalidJwtException("Missing subject claim.");

        var email = principal.FindFirst("preferred_username")?.Value
                    ?? principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                    ?? string.Empty;

        var firstName = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname")?.Value ?? string.Empty;
        var lastName = principal.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname")?.Value ?? string.Empty;
        var name = principal.FindFirst("name")?.Value ?? $"{firstName} {lastName}".Trim();

        return new ExternalAuthPayload(
            SubjectId: subjectId,
            Email: email,
            FirstName: firstName,
            LastName: lastName,
            PictureUrl: null,
            Name: name
        );
    }
}