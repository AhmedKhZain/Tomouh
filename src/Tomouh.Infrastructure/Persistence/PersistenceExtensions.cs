using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Tomouh.Domain.Auth.Repositories;
using Tomouh.Domain.UserInterests.Repositories;
using Tomouh.Infrastructure.Persistence.NoSql;
using Tomouh.Infrastructure.Persistence.NoSql.Configurations;
using Tomouh.Infrastructure.Persistence.NoSql.Repositories;
using Tomouh.Infrastructure.Persistence.Sql;
using Tomouh.Infrastructure.Persistence.Sql.Repositories;

namespace Tomouh.Infrastructure.Persistence;

public static class PersistenceExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        //services.AddDbContextFactory<TomouhDbContext>(options =>
        //{
        //    options.UseSqlServer(
        //        configuration.GetConnectionString("TomouhDataConnection"),
        //        sqlOptions =>
        //        {
        //            sqlOptions.EnableRetryOnFailure(
        //                maxRetryCount: 3,
        //                maxRetryDelay: TimeSpan.FromSeconds(15),
        //                errorNumbersToAdd: null);
        //        });
        //});
        //services.AddDbContextFactory<AppSystemDbContext>(options =>
        //{
        //    options.UseSqlServer(
        //        configuration.GetConnectionString("AppSystemDataConnection"),
        //        sqlOptions =>
        //        {
        //            sqlOptions.EnableRetryOnFailure(
        //                maxRetryCount: 3,
        //                maxRetryDelay: TimeSpan.FromSeconds(15),
        //                errorNumbersToAdd: null);
        //        });
        //});
        services.AddDbContext<TomouhDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("TomouhDataConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
                });
        });
        services.AddDbContext<AppSystemDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("AppSystemDataConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(15),
                        errorNumbersToAdd: null);
                });
        });

        typeof(UserMongoConfiguration).Assembly.ApplyMongoConfigurations();

        var mongoConnectionString = configuration.GetConnectionString("TomouhMongoDbConnection");
        var mongoUrl = new MongoUrl(mongoConnectionString);

        services.AddSingleton(mongoUrl);

        services.AddSingleton<IMongoClient>(sp => new MongoClient(sp.GetRequiredService<MongoUrl>()));

        services.AddScoped<TomouhMongoContext>();

        services.AddRepositories();


        return services;


    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserTokenRepository, UserTokenRepository>();
        services.AddScoped<IUserInterestRepository, UserInterestRepository>();
        return services;
    }


}
