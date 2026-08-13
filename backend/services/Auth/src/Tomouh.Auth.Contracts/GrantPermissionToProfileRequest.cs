using Tomouh.Auth.Application.Commands.GrantPermissionToProfile;
using Tomouh.Auth.Domain.Enums;

namespace Tomouh.Auth.Contracts;

public class GrantPermissionToProfileRequest
{
    public string Permission { get; set; } = string.Empty;

    public GrantPermissionToProfileCommand ToCommand(Guid targetUserId, Role role, Guid idempotencyKey)
        => new(targetUserId, role, Permission, idempotencyKey);
}
