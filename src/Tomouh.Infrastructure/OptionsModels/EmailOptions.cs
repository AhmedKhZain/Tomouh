namespace Tomouh.Infrastructure.OptionsModels;

/// <summary>
/// Represents the configuration options required to send emails via SMTP.
/// <para><strong>Example configuration:</strong></para>
/// <code>
/// "EmailSettings": {
///   "Host": "smtp.example.com",
///   "Port": 587,
///   "EnableSsl": true,
///   "UserName": "your-email@example.com",
///   "Password": "your-password",
///   "From": "noreply@example.com"
/// }
/// </code>
/// </summary>
public class EmailOptions
{
    /// <summary>
    /// Gets or sets the SMTP server host address.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// Gets or sets the port number for the SMTP server.
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether SSL should be enabled.
    /// </summary>
    public bool EnableSsl { get; set; }

    /// <summary>
    /// Gets or sets the username for SMTP authentication.
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// Gets or sets the password for SMTP authentication.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Gets or sets the sender email address.
    /// </summary>
    public string From { get; set; } = null!;
}