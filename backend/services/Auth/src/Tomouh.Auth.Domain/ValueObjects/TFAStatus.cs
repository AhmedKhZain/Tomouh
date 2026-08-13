using Tomouh.Shared.Kernel.BaseTypes;

namespace Tomouh.Auth.Domain.ValueObjects;
#region Value Objects

public class TFAStatus : ValueObject
{
    [EqualityComponent]
    public bool IsTFAEnabled { get; init; }

    public DateTime? TFAEnabledAt { get; init; }

    public TFAStatus(bool isTFAEnabled = false, DateTime? tfaEnabledAt = null)
    {
        IsTFAEnabled = isTFAEnabled;
        TFAEnabledAt = tfaEnabledAt;
    }
}

#endregion