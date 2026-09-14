using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

public class EmailStatus : ValueObject
{
    [EqualityComponent]
    public string Email { get; init; }

    [EqualityComponent]
    public bool IsEmailConfirmed { get; init; }

    public DateTime? ConfirmedAt { get; init; }

    [JsonConstructor]
    [BsonConstructor]
    public EmailStatus(string email, bool isEmailConfirmed = false, DateTime? confirmedAt = null)
    {
        Email = email;
        IsEmailConfirmed = isEmailConfirmed;
        ConfirmedAt = confirmedAt;
    }
}