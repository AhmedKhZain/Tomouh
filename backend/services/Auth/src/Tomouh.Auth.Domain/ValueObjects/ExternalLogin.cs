using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

/// <summary>
/// Represents an external OAuth authentication provider linkage (e.g., Google, Facebook, GitHub).
/// </summary>
public class ExternalLogin : ValueObject
{
    /// <summary>
    /// The provider identifier name (e.g., "Google", "Facebook").
    /// </summary>
    [EqualityComponent]
    public string Provider { get; init; }

    /// <summary>
    /// The unique subject identifier issued by the OAuth provider.
    /// </summary>
    [EqualityComponent]
    public string SubjectId { get; init; }

    /// <summary>
    /// The timestamp when this external provider was linked to the account.
    /// </summary>
    public DateTime LinkedAt { get; init; }

    [BsonConstructor]
    [JsonConstructor]
    public ExternalLogin(string provider, string subjectId, DateTime linkedAt)
    {
        Provider = provider;
        SubjectId = subjectId;
        LinkedAt = linkedAt;
    }

    public static ExternalLogin Create(string provider, string subjectId)
    {
        return new ExternalLogin(provider, subjectId, DateTime.UtcNow);
    }
}