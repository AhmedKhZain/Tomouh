using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;
#region Value Objects

public class ConfirmedEmail : ValueObject
{
    [EqualityComponent]
    public string Email { get; init; }

    [EqualityComponent]
    public bool IsEmailConfirmed { get; init; }

    public DateTime? ConfirmedAt { get; init; }

    public ConfirmedEmail(string email, bool isEmailConfirmed = false, DateTime? confirmedAt = null)
    {
        Email = email;
        IsEmailConfirmed = isEmailConfirmed;
        ConfirmedAt = confirmedAt;
    }
}

#endregion