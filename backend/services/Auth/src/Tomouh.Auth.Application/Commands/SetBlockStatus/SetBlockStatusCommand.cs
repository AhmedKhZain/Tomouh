using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.SetBlockStatus;

[Authorize(roles: "Admin", permissions: "Users.Block", requireAnding: true)]
public record SetBlockStatusCommand(
    Guid TargetUserId,
    bool IsBlocked,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
