using Common.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Tomouh.Domain.Auth;

[JsonConverter(typeof(RoleJsonConverter))]
public class Role : SmartEnum<Role>
{
    public string NormalizedLowerCaseName { get; init; }
    public string NormalizedUpperCaseName { get; init; }
    public List<string> Default { get; init; }

    public static readonly Role SystemOwner = new Role(1, StaticRoleNamesData.SystemOwnerName);

    public static readonly Role SystemAdmin = new Role(2, StaticRoleNamesData.SystemAdminName);
    public static readonly Role User = new Role(3, StaticRoleNamesData.UserName);
    private Role(int value, string name) : base(name, value)
    {
        NormalizedLowerCaseName = name.ToLowerInvariant();
        NormalizedUpperCaseName = name.ToUpperInvariant();
        Default = value switch
        {
            1 => Permissions.Scholarship.All.Concat(Permissions.FundOrganization.All).ToList(),

            2 => Permissions.Scholarship.All.Concat(Permissions.FundOrganization.All).ToList(),

            3 => new List<string>(),

            _ => new List<string>()
        };
    }
    private static readonly List<string> UserDefaultPermissions = new List<string>
    {
        Permissions.Scholarship.Read,
        Permissions.FundOrganization.Read
    };
    public static class StaticRoleNamesData
    {
        public const string SystemOwnerName = "SystemOwner";
        public const string SystemAdminName = "SystemAdmin";
        public const string UserName = "User";
    }

    /// <summary>
    /// Custom JSON converter for Role SmartEnum to handle serialization via Name/FromName.
    /// </summary>
    public class RoleJsonConverter : JsonConverter<Role>
    {
        public override Role? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var roleName = reader.GetString();

                if (string.IsNullOrWhiteSpace(roleName))
                    return null;

                // استخدام FromName المدمجة في Ardalis.SmartEnum
                if (TryFromName(roleName, caseSensitive: false, out var role))
                {
                    return role;
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, Role value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Name);
        }
    }
    public class RoleBsonSerializer : SerializerBase<Role>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Role value)
        {
            if (value is null)
            {
                context.Writer.WriteNull();
                return;
            }

            context.Writer.WriteString(value.Name);
        }

        public override Role Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.CurrentBsonType;

            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }

            if (bsonType == BsonType.String)
            {
                var roleName = context.Reader.ReadString();
                return FromName(roleName, caseSensitive: true);
            }

            throw new BsonSerializationException($"Cannot deserialize Role from BsonType {bsonType}. Expected String.");
        }
    }

}

public static class Permissions
{
    // Scholarship Module
    public static class Scholarship
    {
        public const string Read = "Scholarship.Read";
        public const string Add = "Scholarship.Add";
        public const string Update = "Scholarship.Update";
        public const string Delete = "Scholarship.Delete";
        public const string Restore = "Scholarship.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }

    // Fund Organization Module
    public static class FundOrganization
    {
        public const string Read = "FundOrganization.Read";
        public const string Add = "FundOrganization.Add";
        public const string Update = "FundOrganization.Update";
        public const string Delete = "FundOrganization.Delete";
        public const string Restore = "FundOrganization.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }
    public static class User
    {
        public const string Read = "User.Read";
        public const string Add = "User.Add";
        public const string Update = "User.Update";
        public const string Delete = "User.Delete";
        public const string Restore = "User.Restore";
        public static List<string> All = new() { Read, Add, Update, Delete, Restore };
    }
    public static class UserProfile
    {
        public const string Read = "UserProfile.Read";
        public const string Update = "UserProfile.Update";
        public const string Delete = "UserProfile.Delete";
        public const string Restore = "UserProfile.Restore";
        public const string Add = "UserProfile.Add";
        public static List<string> All = new() { Read, Update, Delete, Restore, Add };
    }
}