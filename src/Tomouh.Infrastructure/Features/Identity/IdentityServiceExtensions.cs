using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Application.Common.Interfaces.Services;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Identity;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Registers core identity & authentication services including Password Hashing, Token Hashing, and Google OAuth services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="authSectionName">The main authentication configuration section name (default is "AuthenticationSettings").</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string authSectionName = "AuthenticationSettings")
    {
        // 1. Register Password & Token Hashing options and services
        services.Configure<LocalPasswordOptions>(configuration.GetSection($"{authSectionName}:PasswordSettings"));
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        // 2. Register Google Auth options and service
        services.Configure<GoogleAuthData>(configuration.GetSection($"{authSectionName}:GoogleAuth"));
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();


        // 3. JWT Generator Configuration
        services.Configure<JwtOptions>(configuration.GetSection($"{authSectionName}:JwtSettings"));
        services.AddScoped<IJwtGenerator, JwtGenerator>();

        return services;
    }
}