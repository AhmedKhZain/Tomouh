using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Commands.Register;

public record RegisterUserCommand(
    string ShowName,
    string FirstName,
    string LastName,
    string Email,
    string Password,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResult>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
