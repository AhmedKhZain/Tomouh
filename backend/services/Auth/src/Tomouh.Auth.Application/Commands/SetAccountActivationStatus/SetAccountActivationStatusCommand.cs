using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.SetAccountActivationStatus;

[Authorize(roles: "Admin", permissions: "Users.ManageStatus", requireAnding: true)]
public record SetAccountActivationStatusCommand(
    Guid TargetUserId,
    bool IsActive,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
