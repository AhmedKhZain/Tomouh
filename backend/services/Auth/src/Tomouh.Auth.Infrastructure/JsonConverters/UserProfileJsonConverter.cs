using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Auth.Domain.ValueObjects;

public class UserProfileJsonConverter : JsonConverter<UserProfile>
{
    private class UserProfileSurrogate
    {
        public Role Role { get; set; } = null!;
        public HashSet<AccountMetadata> Metadata { get; set; } = new();
        public List<string> Permissions { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? LastUpdate { get; set; }
        public Guid? CreatedBy { get; set; }
    }

    public override UserProfile? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var surrogate = JsonSerializer.Deserialize<UserProfileSurrogate>(ref reader, options);
        if (surrogate == null) return null;

        // Bypass constructor logic to avoid business rules/events during deserialization
        var profile = (UserProfile)RuntimeHelpers.GetUninitializedObject(typeof(UserProfile));

        // Set Base Auditable properties using Reflection
        var baseType = typeof(UserProfile).BaseType;
        baseType?.GetProperty("CreatedAt")?.SetValue(profile, surrogate.CreatedAt);
        baseType?.GetProperty("LastUpdate")?.SetValue(profile, surrogate.LastUpdate);
        baseType?.GetProperty("CreatedBy")?.SetValue(profile, surrogate.CreatedBy);

        // Set Domain specific properties
        typeof(UserProfile).GetProperty(nameof(UserProfile.Role))?.SetValue(profile, surrogate.Role);

        // Map Private Fields
        typeof(UserProfile).GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(profile, surrogate.Metadata);

        typeof(UserProfile).GetField("_permissions", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.SetValue(profile, new HashSet<string>(surrogate.Permissions, StringComparer.OrdinalIgnoreCase));

        return profile;
    }

    public override void Write(Utf8JsonWriter writer, UserProfile value, JsonSerializerOptions options)
    {
        var surrogate = new UserProfileSurrogate
        {
            Role = value.Role,
            CreatedAt = value.CreatedAt,
            LastUpdate = value.LastUpdate,
            CreatedBy = value.CreatedBy,

            // Extract private fields
            Metadata = typeof(UserProfile).GetField("_metadata", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(value) as HashSet<AccountMetadata> ?? new(),

            Permissions = new List<string>(value.Permissions)
        };

        JsonSerializer.Serialize(writer, surrogate, options);
    }
}