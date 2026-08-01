using System.Reflection;
using Tomouh.Infrastructure.Persistence.NoSql.Configurations;

namespace Tomouh.Infrastructure.Persistence.NoSql;

public static class MongoMappingExtensions
{
    public static void ApplyMongoConfigurations(this Assembly assembly)
    {
        var configurationTypes = assembly.GetTypes()
            .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IMongoMappingConfiguration))
                     || typeof(IMongoMappingConfiguration).IsAssignableFrom(t))
            .Where(t => t is { IsClass: true, IsAbstract: false });

        foreach (var type in configurationTypes)
        {
            var method = type.GetMethod(nameof(IMongoMappingConfiguration.Configure), BindingFlags.Public | BindingFlags.Static);
            method?.Invoke(null, null);
        }
    }
}
