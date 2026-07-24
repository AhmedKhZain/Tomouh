using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.Enums
{
    public class SmartEnumJsonConverter<TEnum> : JsonConverter<TEnum>
        where TEnum : SmartEnum<TEnum>
    {
        public override TEnum? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number)
            {
                var value = reader.GetInt32();
                return SmartEnum<TEnum>.FromValue(value);
            }

            if (reader.TokenType == JsonTokenType.String)
            {
                var name = reader.GetString();
                return SmartEnum<TEnum>.FromName(name!);
            }

            throw new JsonException($"Invalid token for {typeof(TEnum).Name}");
        }

        public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value.Value);
        }
    }
}
