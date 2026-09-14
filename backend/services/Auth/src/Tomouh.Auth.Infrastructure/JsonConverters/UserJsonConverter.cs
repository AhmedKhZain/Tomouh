using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Auth.Domain.ValueObjects;

namespace Tomouh.Auth.Infrastructure.JsonConverters
{
    public class UserJsonConverter : JsonConverter<User>
    {
        private class UserSurrogate
        {
            public Guid Id { get; set; }
            public Name Name { get; set; } = null!;
            public TFAStatus TFA { get; set; } = null!;
            public EmailStatus MainEmail { get; set; } = null!;
            public AccountStatus Status { get; set; } = null!;
            public string? PasswordHash { get; set; }
            public string? ProfilePhotoPath { get; set; }
            public List<ExternalLogin> ExternalLogins { get; set; } = new();
            public List<UserProfile> Profiles { get; set; } = new();
        }

        public override User? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var surrogate = JsonSerializer.Deserialize<UserSurrogate>(ref reader, options);
            if (surrogate == null) return null;

            // Bypass constructor logic to avoid triggering domain events or validations during deserialization
            var user = (User)RuntimeHelpers.GetUninitializedObject(typeof(User));

            // Map Base Entity properties (Id)
            var baseType = typeof(User).BaseType;
            baseType?.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance)?.SetValue(user, surrogate.Id);

            // Map Domain Value Objects
            typeof(User).GetProperty(nameof(User.Name))?.SetValue(user, surrogate.Name);
            typeof(User).GetProperty(nameof(User.TFA))?.SetValue(user, surrogate.TFA);
            typeof(User).GetProperty(nameof(User.MainEmail))?.SetValue(user, surrogate.MainEmail);
            typeof(User).GetProperty(nameof(User.Status))?.SetValue(user, surrogate.Status);
            typeof(User).GetProperty(nameof(User.ProfilePhotoPath))?.SetValue(user, surrogate.ProfilePhotoPath);

            // Map Private Fields
            typeof(User).GetField("_passwordHash", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(user, surrogate.PasswordHash);
            typeof(User).GetField("_externalLogins", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(user, surrogate.ExternalLogins);
            typeof(User).GetField("_profiles", BindingFlags.NonPublic | BindingFlags.Instance)?.SetValue(user, surrogate.Profiles);

            return user;
        }

        public override void Write(Utf8JsonWriter writer, User value, JsonSerializerOptions options)
        {
            var surrogate = new UserSurrogate
            {
                Id = value.Id,
                Name = value.Name,
                TFA = value.TFA,
                MainEmail = value.MainEmail,
                Status = value.Status,
                ProfilePhotoPath = value.ProfilePhotoPath,

                // Extract private fields using reflection
                PasswordHash = typeof(User).GetField("_passwordHash", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(value) as string,
                ExternalLogins = typeof(User).GetField("_externalLogins", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(value) as List<ExternalLogin> ?? new(),
                Profiles = typeof(User).GetField("_profiles", BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(value) as List<UserProfile> ?? new()
            };

            JsonSerializer.Serialize(writer, surrogate, options);
        }
    }

}
