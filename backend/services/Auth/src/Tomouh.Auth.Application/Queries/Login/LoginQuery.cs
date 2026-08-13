using Tomouh.Auth.Application.Common;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Queries.Login;

public record LoginQuery(
    string Email,
    string Password)
    : IQuery<ResultOf<AuthenticationResult>>,
    IEventsIncludedRequest;
