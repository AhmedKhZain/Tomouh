using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

public class TFAStatus : ValueObject
{
    [EqualityComponent]
    public bool IsTFAEnabled { get; init; }

    public DateTime? TFAEnabledAt { get; init; }
    [JsonConstructor]
    [BsonConstructor]
    public TFAStatus(bool isTFAEnabled = false, DateTime? tfaEnabledAt = null)
    {
        IsTFAEnabled = isTFAEnabled;
        TFAEnabledAt = tfaEnabledAt;
    }


}