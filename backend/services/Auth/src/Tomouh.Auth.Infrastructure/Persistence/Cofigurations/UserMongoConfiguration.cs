using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.ValueObjects;
using Tomouh.Shared.Infrastructure.Features.Persistence;

namespace Tomouh.Auth.Infrastructure.Persistence.Cofigurations;

public class UserMongoConfiguration : IMongoMappingConfiguration
{
    public static void Configure()
    {
        // 0. Register Role Custom Serializer
        try
        {
            BsonSerializer.RegisterSerializer(typeof(Role), new Role.RoleBsonSerializer());
        }
        catch
        {

        }

        // 1. Configure ExternalLogin ClassMap
        if (!BsonClassMap.IsClassMapRegistered(typeof(ExternalLogin)))
        {
            BsonClassMap.RegisterClassMap<ExternalLogin>(cm =>
            {
                cm.AutoMap();

                cm.GetMemberMap(x => x.Provider)?.SetElementName("provider");
                cm.GetMemberMap(x => x.SubjectId)?.SetElementName("subjectId");
                cm.GetMemberMap(x => x.LinkedAt)?.SetElementName("linkedAt");
            });
        }

        // 2. Configure UserProfile ClassMap
        if (!BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
        {
            BsonClassMap.RegisterClassMap<UserProfile>(cm =>
            {
                cm.AutoMap();

                // Map Private Backing Fields
                cm.MapField("_metadata").SetElementName("metadata");
                cm.MapField("_permissions").SetElementName("permissions");

                // Unmap ReadOnly Properties
                cm.UnmapProperty(x => x.Metadata);
                cm.UnmapProperty(x => x.Permissions);
            });
        }

        // 3. Configure User ClassMap (Root Document)
        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();

                // Map Primary Key
                var idMemberMap = cm.GetMemberMap(x => x.Id);
                if (idMemberMap != null)
                {
                    idMemberMap.SetElementName("_id");
                    cm.SetIdMember(idMemberMap);
                }
                // Map Private Backing Fields
                cm.MapField("_passwordHash").SetElementName("passwordHash");
                cm.MapField("_profiles").SetElementName("profiles");
                cm.MapField("_externalLogins").SetElementName("externalLogins");

                // Map Value Objects / Properties
                cm.GetMemberMap(x => x.Name)?.SetElementName("name");
                cm.GetMemberMap(x => x.MainEmail)?.SetElementName("mainEmail");

                cm.GetMemberMap(x => x.TFA)?.SetElementName("tfa");
                cm.GetMemberMap(x => x.Status)?.SetElementName("status");
                cm.GetMemberMap(x => x.ProfilePhotoPath)?.SetElementName("profilePhotoPath");

                // Unmap Calculated / Domain State Properties
                cm.UnmapProperty(a => a.FullName);
                cm.UnmapProperty(a => a.ShowName);
                cm.UnmapProperty(a => a.ExternalLogins);
                cm.UnmapProperty(a => a.Profiles);
                // Unmap Inherited Properties via MemberInfo safely:
                UnmapMemberByName(cm, nameof(User.DomainEvents));
                UnmapMemberByName(cm, nameof(User.IntegrationEvents));
            });
        }
    }

    public static async Task RegisterIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var usersCollection = database.GetCollection<User>("users");

        // 1. Compound Unique Index for External Logins (OAuth Providers)
        var externalLoginKeys = Builders<User>.IndexKeys
            .Ascending("externalLogins.provider")
            .Ascending("externalLogins.subjectId");

        var externalLoginIndexOptions = new CreateIndexOptions
        {
            Name = "IX_Users_ExternalLogins_Provider_SubjectId",
            Unique = true,
            Sparse = true
        };

        var externalLoginIndexModel = new CreateIndexModel<User>(externalLoginKeys, externalLoginIndexOptions);

        // 2. Unique Index for User Email
        var emailKeys = Builders<User>.IndexKeys
            .Ascending("mainEmail.email");

        var emailIndexOptions = new CreateIndexOptions
        {
            Name = "IX_Users_Email",
            Unique = true
        };

        var emailIndexModel = new CreateIndexModel<User>(emailKeys, emailIndexOptions);

        await usersCollection.Indexes.CreateManyAsync(
            new[] { externalLoginIndexModel, emailIndexModel },
            cancellationToken
        );
    }
    private static void UnmapMemberByName<T>(BsonClassMap<T> classMap, string memberName)
    {
        var memberMap = classMap.GetMemberMap(memberName);
        if (memberMap != null)
        {
            classMap.UnmapMember(memberMap.MemberInfo);
        }
    }
}