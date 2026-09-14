using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.RemoveExternalLogin;

public record RemoveExternalLoginCommand(
    AuthProvider Provider,
    Guid RequestId)
    : ICommand<ResultOf<Done>>, IIdempotentRequest, IValidateableRequest;
