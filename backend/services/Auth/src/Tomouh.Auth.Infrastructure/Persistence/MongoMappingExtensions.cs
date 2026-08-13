using MongoDB.Driver;
using System.Reflection;
using Tomouh.Shared.Infrastructure.Features.Persistence;

namespace Tomouh.Auth.Infrastructure.Persistence;

public static class MongoMappingExtensions
{
    /// <summary>
    /// Scans the given assembly and invokes Configure() on all classes implementing IMongoMappingConfiguration.
    /// </summary>
    public static void ApplyMongoConfigurations(this Assembly assembly)
    {
        var configurationTypes = GetMappingConfigurationTypes(assembly);

        foreach (var type in configurationTypes)
        {
            var method = type.GetMethod(nameof(IMongoMappingConfiguration.Configure), BindingFlags.Public | BindingFlags.Static);
            method?.Invoke(null, null);
        }
    }

    /// <summary>
    /// Scans the given assembly and creates indexes for all configurations implementing RegisterIndexesAsync.
    /// </summary>
    public static async Task ApplyMongoIndexesAsync(this Assembly assembly, IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var configurationTypes = GetMappingConfigurationTypes(assembly);

        foreach (var type in configurationTypes)
        {
            var method = type.GetMethod(nameof(IMongoMappingConfiguration.RegisterIndexesAsync), BindingFlags.Public | BindingFlags.Static);
            if (method is not null)
            {
                var task = (Task)method.Invoke(null, new object[] { database, cancellationToken })!;
                await task;
            }
        }
    }

    private static IEnumerable<Type> GetMappingConfigurationTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => typeof(IMongoMappingConfiguration).IsAssignableFrom(t)
                     && t is { IsClass: true, IsAbstract: false });
    }




}
