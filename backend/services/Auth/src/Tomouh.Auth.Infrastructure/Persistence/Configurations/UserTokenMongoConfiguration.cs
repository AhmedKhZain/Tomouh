using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Infrastructure.Features.Persistence;

namespace Tomouh.Auth.Infrastructure.Persistence.Configurations;

public class UserTokenMongoConfiguration : IMongoMappingConfiguration
{
    public static void Configure()
    {
        BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserToken)))
            BsonClassMap.RegisterClassMap<UserToken>(cm =>
            {
                cm.AutoMap();

                var idMap = cm.GetMemberMap(x => x.Id);
                idMap?.SetElementName("_id");
                if (idMap is not null)
                    cm.SetIdMember(idMap);

                cm.GetMemberMap(x => x.TokenHash)?.SetElementName("tokenHash");
                cm.GetMemberMap(x => x.UserId)?.SetElementName("userId");
                cm.GetMemberMap(x => x.TokenType)?.SetElementName("tokenType");
                cm.GetMemberMap(x => x.IsUsed)?.SetElementName("isUsed");
                cm.GetMemberMap(x => x.UsedAt)?.SetElementName("usedAt");
                cm.GetMemberMap(x => x.IsRevoked)?.SetElementName("isRevoked");
                cm.GetMemberMap(x => x.RevokeCause)?.SetElementName("revokeCause");
                cm.GetMemberMap(x => x.RevokedAt)?.SetElementName("revokedAt");
                cm.GetMemberMap(x => x.CreatedAt)?.SetElementName("createdAt");
            });
    }

    public static async Task RegisterIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var userTokensCollection = database.GetCollection<UserToken>("Auth.UserTokens");

        var compoundKeys = Builders<UserToken>.IndexKeys
            .Ascending("userId")
            .Ascending("tokenHash")
            .Ascending("tokenType");
        var compoundIndexOptions = new CreateIndexOptions
        {
            Name = "IX_UserTokens_UserId_TokenHash_TokenType"
        };
        var compoundIndexModel = new CreateIndexModel<UserToken>(compoundKeys, compoundIndexOptions);

        var createdAtKeys = Builders<UserToken>.IndexKeys.Ascending("createdAt");
        var createdAtIndexOptions = new CreateIndexOptions
        {
            Name = "IX_UserTokens_CreatedAt"
        };
        var createdAtIndexModel = new CreateIndexModel<UserToken>(createdAtKeys, createdAtIndexOptions);

        // Fetch existing index names
        using var cursor = await userTokensCollection.Indexes.ListAsync(cancellationToken);
        var existingIndexes = await cursor.ToListAsync(cancellationToken);
        var existingIndexNames = existingIndexes.Select(idx => idx["name"].AsString).ToHashSet(StringComparer.Ordinal);

        var indexesToCreate = new List<CreateIndexModel<UserToken>>();

        if (!existingIndexNames.Contains(compoundIndexOptions.Name))
        {
            indexesToCreate.Add(compoundIndexModel);
        }

        if (!existingIndexNames.Contains(createdAtIndexOptions.Name))
        {
            indexesToCreate.Add(createdAtIndexModel);
        }

        if (indexesToCreate.Count > 0)
        {
            await userTokensCollection.Indexes.CreateManyAsync(
                indexesToCreate,
                cancellationToken
            );
        }
    }
}
