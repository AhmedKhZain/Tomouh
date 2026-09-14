using Tomouh.Auth.Contracts.Responses;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.RegisterWithExternalProvider;

public record RegisterWithExternalProviderCommand(
    AuthProvider Provider,
    string Token,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResult>>, IIdempotentRequest, IValidateableRequest;
