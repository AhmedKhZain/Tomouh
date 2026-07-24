using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.DataConvrters;

public sealed class NonFlagsEnumConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        Type enumType =
            Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;

        return enumType.IsEnum &&
               !enumType.IsDefined(typeof(FlagsAttribute), false);
    }

    public override JsonConverter CreateConverter(
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        Type converterType =
            typeof(NonFlagsEnumConverter<>)
                .MakeGenericType(typeToConvert);

        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}