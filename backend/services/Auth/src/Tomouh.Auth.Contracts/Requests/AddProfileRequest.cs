using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts.Requests;

public class AddProfileRequest
{
    public Role Role { get; set; } = null!;
}