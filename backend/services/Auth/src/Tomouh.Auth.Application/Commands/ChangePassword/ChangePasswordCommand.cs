using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.ChangePassword;

[Authorize]
public record ChangePasswordCommand(
    string CurrentPassword,
    string NewPassword,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IValidateableRequest;
