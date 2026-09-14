using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using System.Reflection;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.ValueObjects;
using Tomouh.Shared.Infrastructure.Features.Persistence;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Infrastructure.Persistence.Configurations;

public class UserMongoConfiguration : IMongoMappingConfiguration
{
    public static void Configure()
    {
        BsonSerializer.TryRegisterSerializer(typeof(Guid), new GuidSerializer(GuidRepresentation.Standard));
        BsonSerializer.TryRegisterSerializer(typeof(Role), new Role.RoleBsonSerializer());

        RegisterBaseMaps();

        if (!BsonClassMap.IsClassMapRegistered(typeof(ExternalLogin)))
            BsonClassMap.RegisterClassMap<ExternalLogin>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.Provider)?.SetElementName("provider");
                cm.GetMemberMap(x => x.SubjectId)?.SetElementName("subjectId");
                cm.GetMemberMap(x => x.LinkedAt)?.SetElementName("linkedAt");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(ExternalLogin).GetProperty(nameof(ExternalLogin.Provider))!,
                    typeof(ExternalLogin).GetProperty(nameof(ExternalLogin.SubjectId))!,
                    typeof(ExternalLogin).GetProperty(nameof(ExternalLogin.LinkedAt))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(Name)))
            BsonClassMap.RegisterClassMap<Name>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.ShowName)?.SetElementName("showName");
                cm.GetMemberMap(x => x.FirstName)?.SetElementName("firstName");
                cm.GetMemberMap(x => x.LastName)?.SetElementName("lastName");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(Name).GetProperty(nameof(Name.ShowName))!,
                    typeof(Name).GetProperty(nameof(Name.FirstName))!,
                    typeof(Name).GetProperty(nameof(Name.LastName))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(EmailStatus)))
            BsonClassMap.RegisterClassMap<EmailStatus>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.Email)?.SetElementName("email");
                cm.GetMemberMap(x => x.IsEmailConfirmed)?.SetElementName("isEmailConfirmed");
                cm.GetMemberMap(x => x.ConfirmedAt)?.SetElementName("confirmedAt");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(EmailStatus).GetProperty(nameof(EmailStatus.Email))!,
                    typeof(EmailStatus).GetProperty(nameof(EmailStatus.IsEmailConfirmed))!,
                    typeof(EmailStatus).GetProperty(nameof(EmailStatus.ConfirmedAt))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(TFAStatus)))
            BsonClassMap.RegisterClassMap<TFAStatus>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.IsTFAEnabled)?.SetElementName("isTFAEnabled");
                cm.GetMemberMap(x => x.TFAEnabledAt)?.SetElementName("tfaEnabledAt");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(TFAStatus).GetProperty(nameof(TFAStatus.IsTFAEnabled))!,
                    typeof(TFAStatus).GetProperty(nameof(TFAStatus.TFAEnabledAt))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(AccountStatus)))
            BsonClassMap.RegisterClassMap<AccountStatus>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.IsActive)?.SetElementName("isActive");
                cm.GetMemberMap(x => x.IsCommentingDisabled)?.SetElementName("isCommentingDisabled");
                cm.GetMemberMap(x => x.CommentingDisabledAt)?.SetElementName("commentingDisabledAt");
                cm.GetMemberMap(x => x.IsBlocked)?.SetElementName("isBlocked");
                cm.GetMemberMap(x => x.BlockedAt)?.SetElementName("blockedAt");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(AccountStatus).GetProperty(nameof(AccountStatus.IsActive))!,
                    typeof(AccountStatus).GetProperty(nameof(AccountStatus.IsCommentingDisabled))!,
                    typeof(AccountStatus).GetProperty(nameof(AccountStatus.CommentingDisabledAt))!,
                    typeof(AccountStatus).GetProperty(nameof(AccountStatus.IsBlocked))!,
                    typeof(AccountStatus).GetProperty(nameof(AccountStatus.BlockedAt))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
            BsonClassMap.RegisterClassMap<UserProfile>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(GetField<UserProfile>("_metadata")).SetElementName("metadata");
                cm.MapMember(GetField<UserProfile>("_permissions")).SetElementName("permissions");
                cm.GetMemberMap(x => x.Role)?.SetElementName("role");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(UserProfile).GetProperty(nameof(UserProfile.Role))!,
                    GetField<UserProfile>("_metadata"),
                    GetField<UserProfile>("_permissions"),
                    typeof(BaseEntity<Role>).GetProperty(nameof(BaseEntity<Role>.CreatedAt))!,
                    typeof(AuditableEntity<Role>).GetProperty(nameof(AuditableEntity<Role>.LastUpdate))!,
                    typeof(BaseEntity<Role>).GetProperty(nameof(BaseEntity<Role>.CreatedBy))!
                });
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();
                cm.MapMember(GetField<User>("_passwordHash")).SetElementName("passwordHash");
                cm.MapMember(GetField<User>("_profiles")).SetElementName("profiles");
                cm.MapMember(GetField<User>("_externalLogins")).SetElementName("externalLogins");
                cm.GetMemberMap(x => x.Name)?.SetElementName("name");
                cm.GetMemberMap(x => x.MainEmail)?.SetElementName("mainEmail");
                cm.GetMemberMap(x => x.TFA)?.SetElementName("tfa");
                cm.GetMemberMap(x => x.Status)?.SetElementName("status");
                cm.GetMemberMap(x => x.ProfilePhotoPath)?.SetElementName("profilePhotoPath");
                ConfigureCreator(cm, new MemberInfo[]
                {
                    typeof(BaseEntity<Guid>).GetProperty(nameof(BaseEntity<Guid>.Id))!,
                    typeof(User).GetProperty(nameof(User.Name))!,
                    typeof(User).GetProperty(nameof(User.MainEmail))!,
                    typeof(User).GetProperty(nameof(User.TFA))!,
                    typeof(User).GetProperty(nameof(User.Status))!,
                    GetField<User>("_passwordHash"),
                    GetField<User>("_profiles"),
                    GetField<User>("_externalLogins"),
                    typeof(User).GetProperty(nameof(User.ProfilePhotoPath))!,
                    typeof(AuditableEntity<Guid>).GetProperty(nameof(AuditableEntity<Guid>.LastUpdate))!,
                    typeof(BaseEntity<Guid>).GetProperty(nameof(BaseEntity<Guid>.CreatedAt))!,
                    typeof(BaseEntity<Guid>).GetProperty(nameof(BaseEntity<Guid>.CreatedBy))!
                });
            });
    }

    public static async Task RegisterIndexesAsync(IMongoDatabase database, CancellationToken cancellationToken = default)
    {
        var usersCollection = database.GetCollection<User>("Users");

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

        var emailKeys = Builders<User>.IndexKeys.Ascending("mainEmail.email");
        var emailIndexOptions = new CreateIndexOptions
        {
            Name = "IX_Users_Email",
            Unique = true
        };
        var emailIndexModel = new CreateIndexModel<User>(emailKeys, emailIndexOptions);

        // Fetch existing index names
        using var cursor = await usersCollection.Indexes.ListAsync(cancellationToken);
        var existingIndexes = await cursor.ToListAsync(cancellationToken);
        var existingIndexNames = existingIndexes.Select(idx => idx["name"].AsString).ToHashSet(StringComparer.Ordinal);

        var indexesToCreate = new List<CreateIndexModel<User>>();

        if (!existingIndexNames.Contains(externalLoginIndexOptions.Name))
        {
            indexesToCreate.Add(externalLoginIndexModel);
        }

        if (!existingIndexNames.Contains(emailIndexOptions.Name))
        {
            indexesToCreate.Add(emailIndexModel);
        }

        if (indexesToCreate.Count > 0)
        {
            await usersCollection.Indexes.CreateManyAsync(
                indexesToCreate,
                cancellationToken
            );
        }
    }

    private static void RegisterBaseMaps()
    {
        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity<Guid>)))
            BsonClassMap.RegisterClassMap<BaseEntity<Guid>>(cm =>
            {
                cm.AutoMap();
                var id = cm.GetMemberMap(x => x.Id);
                id?.SetElementName("_id");
                if (id is not null)
                    cm.SetIdMember(id);
                cm.GetMemberMap(x => x.CreatedBy)?.SetElementName("createdBy");
                cm.GetMemberMap(x => x.CreatedAt)?.SetElementName("createdAt");
                cm.GetMemberMap(x => x.IsDeleted)?.SetElementName("isDeleted");
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(AuditableEntity<Guid>)))
            BsonClassMap.RegisterClassMap<AuditableEntity<Guid>>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.LastUpdate)?.SetElementName("lastUpdate");
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(BaseEntity<Role>)))
            BsonClassMap.RegisterClassMap<BaseEntity<Role>>(cm =>
            {
                cm.AutoMap();
                var id = cm.GetMemberMap(x => x.Id);
                if (id is not null)
                    cm.UnmapMember(id.MemberInfo);
                cm.GetMemberMap(x => x.CreatedBy)?.SetElementName("createdBy");
                cm.GetMemberMap(x => x.CreatedAt)?.SetElementName("createdAt");
                cm.GetMemberMap(x => x.IsDeleted)?.SetElementName("isDeleted");
            });

        if (!BsonClassMap.IsClassMapRegistered(typeof(AuditableEntity<Role>)))
            BsonClassMap.RegisterClassMap<AuditableEntity<Role>>(cm =>
            {
                cm.AutoMap();
                cm.GetMemberMap(x => x.LastUpdate)?.SetElementName("lastUpdate");
            });
    }

    private static void ConfigureCreator<TClass>(BsonClassMap<TClass> cm, MemberInfo[] arguments)
    {
        var creator = cm.CreatorMaps.FirstOrDefault();
        if (creator is null)
        {
            var ctor = typeof(TClass).GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault()
                ?? typeof(TClass).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
                    .FirstOrDefault();
            if (ctor is not null)
                creator = cm.MapConstructor(ctor);
        }
        creator?.SetArguments(arguments);
    }

    private static FieldInfo GetField<T>(string name)
        => typeof(T).GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)!;
}
