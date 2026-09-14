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
    public AccountMetadata(string key, string value, bool isPublic, AccountMetadataType? metadataType = null)
    {
        Key = key;
        Value = value;
        IsPublic = isPublic;
        MetadataType = metadataType;
        CreatedAt = DateTime.UtcNow;
    }
    [JsonConstructor]
    [BsonConstructor]
    private AccountMetadata(string key, string value, bool isPublic, DateTime createdAt, string? metadataType = null)
    {
        Key = key;
        Value = value;
        IsPublic = isPublic;
        if (metadataType != null)
            MetadataType = Enum.Parse<AccountMetadataType>(metadataType);
        CreatedAt = createdAt;
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
