using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.Enums;

namespace Tomouh.Auth.Domain.Enums;

[JsonConverter(typeof(RoleJsonConverter))]
[BsonSerializer(typeof(RoleBsonSerializer))]
public class Role : SmartEnum<Role>
{
    [BsonIgnore]
    public string NormalizedLowerCaseName { get; init; }
    [BsonIgnore]
    public string NormalizedUpperCaseName { get; init; }
    [BsonIgnore]
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
    /// Resilient JSON converter for Role SmartEnum supporting String, Number, and Object fallback.
    /// </summary>
    public class RoleJsonConverter : JsonConverter<Role>
    {
        public override Role? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Case 1: Standard String Representation ("User")
            if (reader.TokenType == JsonTokenType.String)
            {
                var roleName = reader.GetString();
                if (!string.IsNullOrWhiteSpace(roleName) && TryFromName(roleName, out var role, caseSensitive: false))
                {
                    return role;
                }
            }
            // Case 2: Number Representation (3)
            else if (reader.TokenType == JsonTokenType.Number)
            {
                if (reader.TryGetInt32(out var value) && TryFromValue(value, out var role))
                {
                    return role;
                }
            }
            // Case 3: Legacy JSON Object Representation ({ "Name": "User", "Value": 3 })
            else if (reader.TokenType == JsonTokenType.StartObject)
            {
                using var jsonDoc = JsonDocument.ParseValue(ref reader);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("Name", out var nameProp) && nameProp.ValueKind == JsonValueKind.String)
                {
                    var roleName = nameProp.GetString();
                    if (!string.IsNullOrWhiteSpace(roleName) && TryFromName(roleName, out var role, caseSensitive: false))
                    {
                        return role;
                    }
                }

                if (root.TryGetProperty("Value", out var valueProp) && valueProp.ValueKind == JsonValueKind.Number)
                {
                    if (valueProp.TryGetInt32(out var value) && TryFromValue(value, out var role))
                    {
                        return role;
                    }
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

    /// <summary>
    /// BSON Serializer for MongoDB supporting String and Int32 formats.
    /// </summary>
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
                return FromName(roleName, caseSensitive: false);
            }

            if (bsonType == BsonType.Int32)
            {
                var roleValue = context.Reader.ReadInt32();
                return FromValue(roleValue);
            }

            throw new BsonSerializationException($"Cannot deserialize Role from BsonType {bsonType}. Expected String or Int32.");
        }
    }
}