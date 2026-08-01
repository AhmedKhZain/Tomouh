namespace Tomouh.Infrastructure.OptionsModels;

/// <summary>
/// Configuration options for password validation rules.
/// <para><strong>Example configuration:</strong></para>
/// <code>
/// "PasswordSettings": {
///   "MinimumLength": 8,
///   "MaximumLength": 64,
///   "EnforceUppercase": true,
///   "EnforceLowercase": true,
///   "EnforceDigit": true,
///   "EnforceDelimiter": true
/// }
/// </code>
/// </summary>
public class LocalPasswordOptions
{
    /// <summary>Minimum length of the password.</summary>
    public int MinimumLength { get; set; } = 8;

    /// <summary>Maximum length of the password.</summary>
    public int MaximumLength { get; set; } = 64;

    /// <summary>Requires at least one uppercase letter.</summary>
    public bool EnforceUppercase { get; set; } = true;

    /// <summary>Requires at least one lowercase letter.</summary>
    public bool EnforceLowercase { get; set; } = true;

    /// <summary>Requires at least one numeric digit.</summary>
    public bool EnforceDigit { get; set; } = true;

    /// <summary>Requires at least one special character (delimiter).</summary>
    public bool EnforceDelimiter { get; set; } = true;
}