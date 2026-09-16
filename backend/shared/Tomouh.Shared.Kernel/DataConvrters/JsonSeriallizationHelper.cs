using System.Text.Json;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Shared.Kernel.DataConvrters;

/// <summary>
/// Provides JSON serialization and deserialization helpers for types implementing <see cref="IAuditable"/>.
/// Supports caching custom serialization options per type.
/// </summary>
public static class JsonSerializationHelper
{
    /// <summary>
    /// Global Options configured once for all Domain Models & Audit Logging
    /// </summary>
    public static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        IncludeFields = true, // Includes public and [JsonInclude] private fields
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    public static void AddConverter(JsonConverter converter)
    {
        if (!DefaultOptions.Converters.Contains(converter))
        {
            DefaultOptions.Converters.Add(converter);
        }
    }

    public static string Serialize<T>(this T? value) where T : IAuditable
    {
        if (value is null)
            return "null";

        return JsonSerializer.Serialize(value, value.GetType(), DefaultOptions);
    }

    public static T? Deserialize<T>(this string? json) where T : IAuditable
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }
}