using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Queries.Login;

public record LoginQuery(
    string Email,
    string Password)
    : IQuery<ResultOf<AuthenticationResultBase>>;
