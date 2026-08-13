using Tomouh.Auth.Application.Common;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.AddExternalLogin;


public record AddExternalLoginCommand(
    AuthProvider Provider,
    string Token,
    Guid RequestId)
    : ICommand<ResultOf<Done>>, IIdempotentRequest, IValidateableRequest;
