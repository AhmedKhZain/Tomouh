using Common.BaseTypes;
using Common.Markups;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Common.DataConverters;

/// <summary>
/// Provides JSON serialization and deserialization helpers for types implementing <see cref="IAuditable"/>.
/// Supports caching custom serialization options per type.
/// </summary>
public static class JsonSerializationHelper
{
    /// <summary>
    /// Default JSON serialization options used across the application.
    /// </summary>
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        ReferenceHandler = ReferenceHandler.IgnoreCycles,
        WriteIndented = false
    };

    /// <summary>
    /// Thread-safe dictionary cache for resolving type-specific <see cref="JsonSerializerOptions"/>.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, JsonSerializerOptions> OptionsCache = new();

    /// <summary>
    /// Serializes an object implementing <see cref="IAuditable"/> into a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize, constrained to <see cref="IAuditable"/>.</typeparam>
    /// <param name="value">The object instance to serialize.</param>
    /// <returns>A JSON string representation of the object, or "null" if the object is null.</returns>
    public static string Serialize<T>(this T? value)
        where T : IAuditable
    {
        if (value is null)
            return "null";

        var options = value is IHasCustomSerializationOptions customOptionsProvider
            ? customOptionsProvider.Options
            : GetOptionsForType(value.GetType());

        return JsonSerializer.Serialize(value, value.GetType(), options);
    }

    /// <summary>
    /// Deserializes a JSON string into an instance of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The target type to deserialize into, constrained to <see cref="IAuditable"/>.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An instance of <typeparamref name="T"/>, or default if the JSON string is null or white space.</returns>
    public static T? Deserialize<T>(this string? json)
        where T : IAuditable
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        var options = GetOptionsForType(typeof(T));

        return JsonSerializer.Deserialize<T>(json, options);
    }

    /// <summary>
    /// Resolves and caches the <see cref="JsonSerializerOptions"/> for a given type.
    /// </summary>
    /// <param name="type">The target type to resolve options for.</param>
    /// <returns>The cached <see cref="JsonSerializerOptions"/> or <see cref="DefaultOptions"/>.</returns>
    private static JsonSerializerOptions GetOptionsForType(Type type)
    {
        return OptionsCache.GetOrAdd(type, targetType =>
        {
            if (typeof(IHasCustomSerializationOptions).IsAssignableFrom(targetType))
            {
                var instance = (IHasCustomSerializationOptions)Activator.CreateInstance(targetType)!;
                return instance.Options;
            }

            return DefaultOptions;
        });
    }
}