using MongoDB.Bson.Serialization;
using System.Reflection;
using Tomouh.Domain.Auth;

namespace Tomouh.Infrastructure.Persistence.NoSql.Configurations;

public static class UserMongoConfiguration
{
    public static void Configure()
    {
        // Register Role Custom Serializer
        BsonSerializer.RegisterSerializer(typeof(Role), new Role.RoleBsonSerializer());

        // 1. Configure UserProfile ClassMap
        if (!BsonClassMap.IsClassMapRegistered(typeof(UserProfile)))
        {
            BsonClassMap.RegisterClassMap<UserProfile>(cm =>
            {
                cm.AutoMap();

                // Get internal constructor
                var constructor = typeof(UserProfile)
                    .GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                    .FirstOrDefault(c => c.GetParameters().Length > 0);

                if (constructor is not null)
                {
                    // Map parameters dynamically using actual parameter names
                    var parameterNames = constructor.GetParameters()
                        .Select(p => p.Name)
                        .ToArray();

                    cm.MapConstructor(constructor, parameterNames);
                }

                // Map members & set BSON element names
                cm.GetMemberMap(x => x.Role)?.SetElementName("role");
                cm.GetMemberMap(x => x.Metadata)?.SetElementName("metadata");
                cm.GetMemberMap(x => x.Permissions)?.SetElementName("permissions");

                // Unmap Domain Properties / Events
                cm.UnmapProperty(a => a.IsUpdated);
            });
        }

        // 2. Configure User ClassMap (Root Document)
        if (!BsonClassMap.IsClassMapRegistered(typeof(User)))
        {
            BsonClassMap.RegisterClassMap<User>(cm =>
            {
                cm.AutoMap();

                // Map Primary Key
                cm.MapIdProperty(x => x.Id).SetElementName("_id");

                // Map Private Fields directly
                cm.MapField("_passwordHash").SetElementName("passwordHash");
                cm.MapField("_profiles").SetElementName("profiles");

                // Map Value Objects / Properties
                cm.GetMemberMap(x => x.Name)?.SetElementName("name");
                cm.GetMemberMap(x => x.Email)?.SetElementName("email");
                cm.GetMemberMap(x => x.TFA)?.SetElementName("tfa");
                cm.GetMemberMap(x => x.Status)?.SetElementName("status");
                cm.GetMemberMap(x => x.AuthProvider)?.SetElementName("authProvider");
                cm.GetMemberMap(x => x.ProviderSubjectId)?.SetElementName("providerSubjectId");
                cm.GetMemberMap(x => x.ProfilePhotoPath)?.SetElementName("profilePhotoPath");
                // Unmap Calculated / Domain State Properties
                cm.UnmapProperty(a => a.FullName);
                cm.UnmapProperty(a => a.ShowName);
                cm.UnmapProperty(a => a.DomainEvents);
                cm.UnmapProperty(a => a.IntegrationEvents);
                cm.UnmapProperty(a => a.IsUpdated);
                cm.UnmapProperty(a => a.Options);


            });
        }
    }
}