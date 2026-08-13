using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MongoDB.Driver;

namespace Tomouh.Auth.Infrastructure.Persistence;

public static class MongoInitializationExtensions
{
    public static async Task UseMongoInitializationAsync(this IHost app)
    {
        var assembly = typeof(MongoInitializationExtensions).Assembly;
        assembly.ApplyMongoConfigurations();

        using var scope = app.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<IMongoDatabase>();

        await assembly.ApplyMongoIndexesAsync(database);
    }
}