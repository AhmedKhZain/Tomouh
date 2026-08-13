using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.GrantPermissionToProfile;

[Authorize(roles: "Admin", permissions: "Permissions.Grant", requireAnding: true)]
public record GrantPermissionToProfileCommand(
    Guid TargetUserId,
    Role Role,
    string Permission,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
