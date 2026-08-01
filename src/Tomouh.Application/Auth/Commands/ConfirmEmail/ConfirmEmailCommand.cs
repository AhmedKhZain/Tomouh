using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Queries.ConfirmEmail;

public record ConfirmEmailCommand(
    string Token,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResult>>,
    IIdempotentRequest,
    IValidateableRequest;
