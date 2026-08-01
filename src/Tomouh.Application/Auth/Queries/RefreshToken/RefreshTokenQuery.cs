using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Queries.RefreshToken;

public record RefreshTokenQuery()
    : ICommand<ResultOf<AuthenticationResult>>;
