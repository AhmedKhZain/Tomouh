using Tomouh.Auth.Application.Common;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Queries.RefreshToken;

public record RefreshTokenQuery()
    : ICommand<ResultOf<AuthenticationResult>>;
