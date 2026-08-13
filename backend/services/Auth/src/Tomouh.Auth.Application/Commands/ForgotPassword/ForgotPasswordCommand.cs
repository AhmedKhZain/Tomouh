using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(
    string Email,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IValidateableRequest,
    IIdempotentRequest,
    IEventsIncludedRequest;
