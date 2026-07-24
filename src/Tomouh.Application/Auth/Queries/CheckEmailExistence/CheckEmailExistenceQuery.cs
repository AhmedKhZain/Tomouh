using Common.Requests;
using Common.ResultOf;
using Tomouh.Application.Auth.Common;

namespace Tomouh.Application.Auth.Queries.CheckEmailExistence;

public record CheckEmailExistenceQuery(string Email, Guid RequestId)
    : IQuery<ResultOf<AuthenticationResultBase>>,
    IIdempotentRequest,
    IEventsIncludedRequest,
    IValidateableRequest;



