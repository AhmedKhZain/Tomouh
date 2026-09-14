using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomouh.Shared.Infrastructure.Features.Persistence.Contexts;
using Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Shared.Infrastructure.Features.Persistence;

public static class PersistenceServiceExtensions
{
    public static IServiceCollection AddLocalNoSqlUnitOfWorkImplementation<T>(this IServiceCollection services)
        where T : MongoBaseContext, IUnitOfWork
    {
        services.AddScoped<IUnitOfWork, T>();
        services.AddScoped<T>();
        return services;
    }
    public static IServiceCollection AddLocalSqlUnitOfWorkImplementation<T>(this IServiceCollection services, Action<DbContextOptionsBuilder> optionsAction)
        where T : DbBaseContext, IUnitOfWork
    {
        services.AddScoped<IUnitOfWork, T>();
        services.AddDbContextFactory<T>(optionsAction);

        return services;
    }
}
