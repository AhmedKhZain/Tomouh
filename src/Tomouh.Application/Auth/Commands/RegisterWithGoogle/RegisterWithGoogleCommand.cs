using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Commands.RegisterWithGoogle;

public record RegisterWithGoogleCommand(
    string GoogleToken,
    Guid RequestId)
    : ICommand<ResultOf<AuthenticationResult>>, IIdempotentRequest, IValidateableRequest;
