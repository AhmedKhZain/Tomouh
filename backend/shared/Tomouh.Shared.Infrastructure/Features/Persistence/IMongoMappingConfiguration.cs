using MongoDB.Driver;

namespace Tomouh.Shared.Infrastructure.Features.Persistence;

public interface IMongoMappingConfiguration
{
    /// <summary>
    /// Configures BSON class maps and custom serializers for Mongo.
    /// </summary>
    static abstract void Configure();

    /// <summary>
    /// Creates Mongo collection indexes if needed. (Optional override)
    /// </summary>
    static virtual Task RegisterIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}