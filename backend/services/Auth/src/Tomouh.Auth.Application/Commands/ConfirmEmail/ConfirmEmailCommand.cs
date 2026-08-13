using Tomouh.Auth.Application.Common;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.ConfirmEmail;

public record ConfirmEmailCommand(
    string Token,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResult>>,
    IIdempotentRequest,
    IValidateableRequest;
