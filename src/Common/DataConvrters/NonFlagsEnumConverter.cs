using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.DataConvrters;

public sealed class NonFlagsEnumConverter<T> : JsonConverter<T>
{
    private static readonly Type TypeOfT = typeof(T);
    private static readonly Type EnumType =
        Nullable.GetUnderlyingType(TypeOfT) ?? TypeOfT;

    private static readonly bool IsNullable =
        Nullable.GetUnderlyingType(TypeOfT) is not null;

    public override T? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        // null
        if (reader.TokenType == JsonTokenType.Null)
        {
            if (IsNullable)
                return default;

            throw new JsonException(
                $"Cannot convert null to non-nullable enum '{EnumType.Name}'.");
        }

        // "EnumName"
        if (reader.TokenType == JsonTokenType.String)
        {
            string? enumName = reader.GetString();

            if (Enum.TryParse(EnumType, enumName, true, out object? parsed))
                return (T)parsed!;

            throw new JsonException(
                $"'{enumName}' is not a valid value for enum '{EnumType.Name}'.");
        }

        throw new JsonException(
            $"Unexpected token {reader.TokenType} when parsing enum '{EnumType.Name}'.");
    }

    public override void Write(
        Utf8JsonWriter writer,
        T value,
        JsonSerializerOptions options)
    {
        // nullable enum = null
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStringValue(value.ToString());
    }
}
