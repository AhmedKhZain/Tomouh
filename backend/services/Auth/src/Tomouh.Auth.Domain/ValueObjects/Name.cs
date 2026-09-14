using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;
using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;

public class Name : ValueObject
{
    [EqualityComponent]
    public string ShowName { get; init; }

    [EqualityComponent]
    public string FirstName { get; init; }

    [EqualityComponent]
    public string LastName { get; init; }

    [JsonConstructor]
    [BsonConstructor]
    public Name(string showName, string firstName, string lastName)
    {
        ShowName = showName;
        FirstName = firstName;
        LastName = lastName;
    }
}