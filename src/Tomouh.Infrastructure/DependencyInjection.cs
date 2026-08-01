using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Infrastructure.Features.Cache;
using Tomouh.Infrastructure.Features.Email;
using Tomouh.Infrastructure.Features.Identity;
using Tomouh.Infrastructure.Persistence;

namespace Tomouh.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IDomainEventCollector, DomainEventCollector>();
        return services.AddIdentityInfrastructure(configuration)
            .AddEmailServices(configuration)
            .AddCaching(configuration)
            .AddPersistence(configuration);
    }
}
