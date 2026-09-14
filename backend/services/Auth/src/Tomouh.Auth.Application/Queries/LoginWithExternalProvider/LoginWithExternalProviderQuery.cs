using Tomouh.Auth.Contracts.Responses;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Queries.LoginWithExternalProvider;

public record LoginWithExternalProviderQuery(
    AuthProvider Provider,
    string Token,
    Guid RequestId)
    : IQuery<ResultOf<AuthenticationResult>>, IIdempotentRequest, IValidateableRequest;
