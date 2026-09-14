namespace Tomouh.Auth.Contracts.Requests;

public class ChangeTFAStatusRequest
{
    public bool IsEnabled { get; set; }
    public string Password { get; set; } = string.Empty;
}