using Common.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Infrastructure.OptionsModels;

namespace Tomouh.Infrastructure.Features.Email;

public static class EmailServiceExtensions
{
    private const string DefaultSettingsSection = "EmailSettings";

    /// <summary>
    /// Registers email services using configuration from the appsettings file.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The root configuration object.</param>
    /// <param name="sectionName">The name of the configuration section.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = DefaultSettingsSection)
    {
        services.Configure<EmailOptions>(configuration.GetSection(sectionName));
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }

    /// <summary>
    /// Registers email services using a hardcoded or runtime settings object.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="emailSettings">The email settings instance.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddEmailServices(
        this IServiceCollection services,
        EmailOptions emailSettings)
    {
        services.AddSingleton(emailSettings);
        services.AddScoped<IEmailSender, EmailSender>();

        return services;
    }
}