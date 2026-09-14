using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

public class AccountStatus : ValueObject
{
    [EqualityComponent]
    public bool IsActive { get; init; }

    [EqualityComponent]
    public bool IsCommentingDisabled { get; init; }

    public DateTime? CommentingDisabledAt { get; init; }

    [EqualityComponent]
    public bool IsBlocked { get; init; }

    public DateTime? BlockedAt { get; init; }
    [JsonConstructor]
    [BsonConstructor]
    public AccountStatus(
        bool isActive = true,
        bool isCommentingDisabled = false,
        DateTime? commentingDisabledAt = null,
        bool isBlocked = false,
        DateTime? blockedAt = null)
    {
        IsActive = isActive;
        IsCommentingDisabled = isCommentingDisabled;
        CommentingDisabledAt = commentingDisabledAt;
        IsBlocked = isBlocked;
        BlockedAt = blockedAt;
    }


}