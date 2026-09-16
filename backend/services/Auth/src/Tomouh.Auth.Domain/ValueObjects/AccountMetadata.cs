using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

public class AccountMetadata : ValueObject
{
    [EqualityComponent]
    public string Key { get; init; }
    public string Value { get; init; }
    public bool IsPublic { get; init; }
    public AccountMetadataType? MetadataType { get; init; }
    public DateTime CreatedAt { get; init; }
    [JsonConstructor]
    [BsonConstructor]
    public AccountMetadata(string key, string value, bool isPublic, DateTime createdAt, AccountMetadataType? metadataType = null)
    {
        Key = key;
        Value = value;
        IsPublic = isPublic;
        MetadataType = metadataType;
        CreatedAt = createdAt;
    }

    /// <summary>
    /// Returns a copy of this metadata entry with the same Key/IsPublic/MetadataType and the provided value.
    /// </summary>
    public AccountMetadata WithValue(string value, bool isPublic, AccountMetadataType? metadataType)
    {
        return new AccountMetadata(Key, value, isPublic, CreatedAt, metadataType);
    }
}
public enum AccountMetadataType
{
    Date = 1,
    String,
    Id,
    Num,
    Name
}