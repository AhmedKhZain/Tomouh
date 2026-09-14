using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts.Requests;

public class AddExternalLoginRequest
{
    public AuthProvider Provider { get; set; }
    public string Token { get; set; } = string.Empty;
}