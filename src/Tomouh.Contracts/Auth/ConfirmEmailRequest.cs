namespace Tomouh.Contracts.Auth;

public class ConfirmEmailRequest
{
    public string UserEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
