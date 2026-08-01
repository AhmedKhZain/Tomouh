using Common.Requests;
using Common.ResultOf;

namespace Tomouh.Application.Auth.Queries.ForgotPassword;

public record ForgotPasswordCommand(
    string Email,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IValidateableRequest,
    IIdempotentRequest,
    IEventsIncludedRequest;
