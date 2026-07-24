using Common.Infrastructure.OptionsModels;
using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastructure.Features.Identity;

public static class IdentityServiceExtensions
{
    /// <summary>
    /// Registers the password hasher with custom configuration section support.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <param name="sectionName">The configuration section name (default is "PasswordSettings").</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddPasswordHasher(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "PasswordSettings")
    {
        services.Configure<PasswordOptions>(configuration.GetSection(sectionName));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        return services;
    }
}
