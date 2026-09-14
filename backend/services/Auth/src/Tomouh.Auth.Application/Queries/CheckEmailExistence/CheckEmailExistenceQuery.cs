using Tomouh.Auth.Contracts.Responses;
using Tomouh.Shared.Kernel.Requests;
using Tomouh.Shared.Kernel.ResultOf;

namespace Tomouh.Auth.Application.Queries.CheckEmailExistence;

public record CheckEmailExistenceQuery(string Email, Guid RequestId)
    : IQuery<ResultOf<AuthenticationResult>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;



