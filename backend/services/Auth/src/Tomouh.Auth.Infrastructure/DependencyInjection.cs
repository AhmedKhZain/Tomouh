using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Security.Cryptography;
using Tomouh.Auth.Application.Interfaces;
using Tomouh.Auth.Domain.Interfaces;
using Tomouh.Auth.Infrastructure.ExternalAuth;
using Tomouh.Auth.Infrastructure.Identity;
using Tomouh.Auth.Infrastructure.Options;
using Tomouh.Auth.Infrastructure.Persistence.Contexts;
using Tomouh.Auth.Infrastructure.Persistence.Repositories;
using Tomouh.Shared.Infrastructure.Features.Cache;
using Tomouh.Shared.Infrastructure.Features.Email;
using Tomouh.Shared.Infrastructure.Features.Identity;
using Tomouh.Shared.Infrastructure.OptionsModels;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Auth.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Single Entry Point to register all Auth Service infrastructure dependencies.
    /// </summary>
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddAuthDatabases(configuration)
            .AddAuthRepositories()
            .AddExternalAuthProviders(configuration)
            .AddPasswordAndTokenHashing(configuration)
            .AddJwtSigningToken(configuration)
            .AddSharedInfrastructure(configuration);

        return services;
    }

    private static IServiceCollection AddAuthDatabases(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IMongoClient>(sp =>
            new MongoClient(configuration.GetConnectionString("MongoDb")));

        services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            return client.GetDatabase(configuration["MongoDb:DatabaseName"]);
        });

        services.AddScoped<AuthContext>();

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AuthContext>());

        return services;
    }

    private static IServiceCollection AddAuthRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();

        return services;
    }

    private static IServiceCollection AddExternalAuthProviders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ExternalAuthSettings>(
            configuration.GetSection($"AuthenticationSettings:{ExternalAuthSettings.SectionName}"));

        services.AddHttpClient("GitHubAuth");

        services.AddTransient<IExternalAuthProviderStrategy, GoogleAuthProviderStrategy>();
        services.AddTransient<IExternalAuthProviderStrategy, GitHubAuthProviderStrategy>();
        services.AddTransient<IExternalAuthProviderStrategy, MicrosoftAuthProviderStrategy>();

        services.AddSingleton<IExternalAuthProviderFactory, ExternalAuthProviderFactory>();

        return services;
    }

    private static IServiceCollection AddPasswordAndTokenHashing(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<LocalPasswordOptions>(
            configuration.GetSection("AuthenticationSettings:PasswordSettings"));

        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ITokenHasher, TokenHasher>();

        return services;
    }

    private static IServiceCollection AddJwtSigningToken(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(
            configuration.GetSection("AuthenticationSettings:JwtSettings"));

        var privateKeyFilePath = Path.Combine(AppContext.BaseDirectory, "private_key.pem");

        if (!File.Exists(privateKeyFilePath))
        {
            throw new FileNotFoundException($"Private Key file not found at: {privateKeyFilePath}. Ensure 'private_key.pem' is set to Copy to Output Directory.");
        }

        var privateKeyPem = File.ReadAllText(privateKeyFilePath);

        var ecdsa = ECDsa.Create();
        ecdsa.ImportFromPem(privateKeyPem.ToCharArray());

        var signingKey = new ECDsaSecurityKey(ecdsa);
        services.AddSingleton(signingKey);

        services.AddScoped<IJwtGenerator, JwtGenerator>();

        return services;
    }

    private static IServiceCollection AddSharedInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCaching(configuration);
        services.AddEmailServices(configuration);
        services.AddSharedCurrentUserProvider();
        services.AddSharedJwtAuthentication(configuration);

        return services;
    }
}