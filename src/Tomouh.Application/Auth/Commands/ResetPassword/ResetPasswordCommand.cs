using Common.Requests;
using Common.ResultOf;

namespace Tomouh.Application.Auth.Queries.ResetPassword;

public record ResetPasswordCommand(
    string Token,
    string NewPassword,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IValidateableRequest;
