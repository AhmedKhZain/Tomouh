using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using Tomouh.Shared.Infrastructure.OptionsModels;
using Tomouh.Shared.Kernel.Features;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Shared.Infrastructure.Features.Identity;

public static class SharedIdentityServiceExtensions
{
    private const string DefaultJwtSectionPath = "AuthenticationSettings:JwtSettings";

    /// <summary>
    /// Registers shared JWT Authentication bearer middleware using ECDSA Public Key validation.
    /// </summary>
    public static IServiceCollection AddSharedJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string jwtSectionPath = DefaultJwtSectionPath)
    {
        var jwtOptions = configuration.GetSection(jwtSectionPath).Get<JwtOptions>()
            ?? throw new InvalidOperationException($"JwtOptions section '{jwtSectionPath}' is missing or invalid in configuration.");

        var publicKeyFilePath = Path.Combine(AppContext.BaseDirectory, "public_key.pem");

        if (!File.Exists(publicKeyFilePath))
        {
            throw new FileNotFoundException($"Public Key file not found at: {publicKeyFilePath}. Ensure 'public_key.pem' is set to Copy to Output Directory.");
        }

        var publicKeyPem = File.ReadAllText(publicKeyFilePath);

        using var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(publicKeyPem.ToCharArray());

        var explicitParams = ecdsa.ExportParameters(false);
        var validationKey = new ECDsaSecurityKey(ECDsa.Create(explicitParams));

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = validationKey,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }

    /// <summary>
    /// Registers shared CurrentUserProvider and scoped CurrentUser instance.
    /// </summary>
    public static IServiceCollection AddSharedCurrentUserProvider(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();
        services.AddScoped<CurrentUser>(sp => sp.GetRequiredService<ICurrentUserProvider>().GetCurrentUser());

        return services;
    }
}