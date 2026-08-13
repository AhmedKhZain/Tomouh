using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.RevokePermissionFromProfile;

[Authorize(roles: "Admin", permissions: "Permissions.Revoke", requireAnding: true)]
public record RevokePermissionFromProfileCommand(
    Guid TargetUserId,
    Role Role,
    string Permission,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
