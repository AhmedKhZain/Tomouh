namespace Tomouh.Auth.Contracts.Requests;

public class ConfirmEmailRequest
{
    public string Token { get; set; } = string.Empty;
}