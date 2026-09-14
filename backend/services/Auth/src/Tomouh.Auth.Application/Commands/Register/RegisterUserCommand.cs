using Tomouh.Auth.Contracts.Responses;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.Register;

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
