using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Queries.LoginWithGoogle;

public record LoginWithGoogleQuery(
    string GoogleToken,
    Guid RequestId)
    : IQuery<ResultOf<AuthenticationResult>>, IIdempotentRequest, IValidateableRequest;
