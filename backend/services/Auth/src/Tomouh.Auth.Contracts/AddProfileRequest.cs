using Tomouh.Auth.Application.Commands.AddProfile;
using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts;

public class AddProfileRequest
{
    public Role Role { get; set; } = null!;

    public AddProfileCommand ToCommand(Guid idempotencyKey)
        => new(Role, idempotencyKey);
}
