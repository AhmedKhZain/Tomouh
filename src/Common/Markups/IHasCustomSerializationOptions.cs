using System.Text.Json;

namespace Common.Markups;

public interface IHasCustomSerializationOptions
{
    JsonSerializerOptions Options { get; }
}