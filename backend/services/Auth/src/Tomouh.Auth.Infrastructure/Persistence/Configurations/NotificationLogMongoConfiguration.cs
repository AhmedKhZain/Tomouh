using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Tomouh.Shared.Infrastructure.Features.Persistence;
using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Auth.Infrastructure.Persistence.Configurations;

public class NotificationLogMongoConfiguration : IMongoMappingConfiguration
{
    public static void Configure()
    {
        BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

        if (!BsonClassMap.IsClassMapRegistered(typeof(EventOutbox)))
            BsonClassMap.RegisterClassMap<EventOutbox>(cm =>
            {
                cm.AutoMap();

                var idMap = cm.GetMemberMap(x => x.Id);
                idMap?.SetElementName("_id");
                if (idMap is not null)
                    cm.SetIdMember(idMap);

                cm.GetMemberMap(x => x.EventTypeName)?.SetElementName("eventTypeName");
                cm.GetMemberMap(x => x.EventType)?.SetElementName("eventType");
                cm.GetMemberMap(x => x.IsHandled)?.SetElementName("isHandled");
                cm.GetMemberMap(x => x.CreatedAt)?.SetElementName("createdAt");
                cm.GetMemberMap(x => x.HandledAt)?.SetElementName("handledAt");
                cm.GetMemberMap(x => x.ErrorMessage)?.SetElementName("errorMessage");
                cm.GetMemberMap(x => x.RetryCount)?.SetElementName("retryCount");
                cm.GetMemberMap(x => x.Notification)?.SetElementName("notification");
                cm.GetMemberMap(x => x.CreatedBy)?.SetElementName("createdBy");
                cm.GetMemberMap(x => x.ActorType)?.SetElementName("actorType");
                cm.GetMemberMap(x => x.SkipMaxRetryAttempts)?.SetElementName("skipMaxRetryAttempts");
                cm.GetMemberMap(x => x.SkipMoreCount)?.SetElementName("skipMoreCount");
            });
    }

    public static async Task RegisterIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var notificationLogsCollection = database.GetCollection<EventOutbox>("Auth.NotificationLogs");

        var keys = Builders<EventOutbox>.IndexKeys
            .Ascending("createdBy")
            .Ascending("createdAt");
        var options = new CreateIndexOptions
        {
            Name = "IX_NotificationLogs_CreatedBy_CreatedAt"
        };

        var indexModel = new CreateIndexModel<EventOutbox>(keys, options);

        // Fetch existing index names
        using var cursor = await notificationLogsCollection.Indexes.ListAsync(cancellationToken);
        var existingIndexes = await cursor.ToListAsync(cancellationToken);
        var existingIndexNames = existingIndexes.Select(idx => idx["name"].AsString).ToHashSet(StringComparer.Ordinal);

        var indexesToCreate = new List<CreateIndexModel<EventOutbox>>();

        if (!existingIndexNames.Contains(options.Name))
        {
            indexesToCreate.Add(indexModel);
        }

        if (indexesToCreate.Count > 0)
        {
            await notificationLogsCollection.Indexes.CreateManyAsync(
                indexesToCreate,
                cancellationToken
            );
        }
    }
}
