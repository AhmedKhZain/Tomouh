using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Commands.AddProfile;

[Authorize]
public record AddProfileCommand(
    Role Role,
    Guid RequestId)
    : ICommand<ResultOf<Done>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;
