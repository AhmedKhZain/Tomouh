using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.AddExternalLogin;

[Authorize]
public record AddExternalLoginCommand(
    AuthProvider Provider,
    string Token,
    Guid RequestId)
    : ICommand<ResultOf<Done>>, IIdempotentRequest, IValidateableRequest;
