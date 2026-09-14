using Tomouh.Auth.Contracts.Responses;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Queries.RefreshToken;

public record RefreshTokenQuery(string? Token = null)
    : ICommand<ResultOf<AuthenticationResult>>;
