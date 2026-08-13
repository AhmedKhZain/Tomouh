using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;
#region Value Objects

public class Name : ValueObject
{
    [EqualityComponent]
    public string ShowName { get; init; }

    [EqualityComponent]
    public string FirstName { get; init; }

    [EqualityComponent]
    public string LastName { get; init; }

    public Name(string showName, string firstName, string lastName)
    {
        ShowName = showName;
        FirstName = firstName;
        LastName = lastName;
    }
}

#endregion