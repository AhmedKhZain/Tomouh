using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.SetCommentingStatus;

[Authorize(permissions: "Users.ManageCommenting")]
public record SetCommentingStatusCommand(
    Guid TargetUserId,
    bool IsDisabled,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
