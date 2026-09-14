using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.Enums;

namespace Tomouh.Auth.Domain.Enums;

[JsonConverter(typeof(TokenTypeJsonConverter))]
[BsonSerializer(typeof(TokenTypeBsonSerializer))]
public class TokenType : SmartEnum<TokenType>
{
    public TimeSpan Expiration { get; private set; }
    public static readonly TimeSpan RefreshTokenExpiration = TimeSpan.FromDays(10);
    public static readonly TimeSpan EmailConfirmationExpiration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan PasswordResetExpiration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan TwoFactorAuthenticationExpiration = TimeSpan.FromMinutes(10);

    public static readonly TokenType RefreshToken = new TokenType("RefreshToken", 1, RefreshTokenExpiration);
    public static readonly TokenType EmailConfirmation = new TokenType("EmailConfirmation", 2, EmailConfirmationExpiration);
    public static readonly TokenType PasswordReset = new TokenType("PasswordReset", 3, PasswordResetExpiration);
    public static readonly TokenType TwoFactorAuthentication = new TokenType("TwoFactorAuthentication", 4, TwoFactorAuthenticationExpiration);

    private TokenType(string name, int value, TimeSpan expiration) : base(name, value)
    {
        Expiration = expiration;
    }

    internal string? Create()
    {
        string token;

        switch (this.Value)
        {
            case 4: // RefreshToken
                token = GenerateSecureRandomString(128);
                break;

            case 1: // EmailConfirmation
                token = GenerateSecureRandomString(6);
                break;
            case 2: // PasswordReset
                token = GenerateSecureRandomString(6);
                break;

            case 3: // TwoFactorAuthentication
                token = GenerateNumericCode(6);
                break;

            default:
                throw new ArgumentException("Unsupported token type");
        }

        return token;
    }

    private static string GenerateSecureRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var data = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(data);
        }

        var result = new StringBuilder(length);
        foreach (byte b in data)
        {
            result.Append(chars[b % chars.Length]);
        }

        return result.ToString();
    }

    private static string GenerateNumericCode(int length)
    {
        var rng = new Random();
        return rng.Next((int)Math.Pow(10, length - 1), (int)Math.Pow(10, length)).ToString();
    }

    public class TokenTypeJsonConverter : JsonConverter<TokenType>
    {
        public override TokenType? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var tokenTypeName = reader.GetString();

                if (string.IsNullOrWhiteSpace(tokenTypeName))
                    return null;

                if (TryFromName(tokenTypeName, out var tokenType, caseSensitive: false))
                {
                    return tokenType;
                }
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, TokenType value, JsonSerializerOptions options)
        {
            if (value is null)
            {
                writer.WriteNullValue();
                return;
            }

            writer.WriteStringValue(value.Name);
        }
    }

    public class TokenTypeBsonSerializer : SerializerBase<TokenType>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TokenType value)
        {
            if (value is null)
            {
                context.Writer.WriteNull();
                return;
            }

            context.Writer.WriteString(value.Name);
        }

        public override TokenType Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonType = context.Reader.CurrentBsonType;

            if (bsonType == BsonType.Null)
            {
                context.Reader.ReadNull();
                return null;
            }

            if (bsonType == BsonType.String)
            {
                var tokenTypeName = context.Reader.ReadString();
                return FromName(tokenTypeName, caseSensitive: true);
            }

            throw new BsonSerializationException($"Cannot deserialize TokenType from BsonType {bsonType}. Expected String.");
        }
    }
}