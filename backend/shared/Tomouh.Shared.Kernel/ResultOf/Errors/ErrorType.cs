namespace Tomouh.Shared.Kernel.ResultOf.Errors;

/// <summary>
/// Error types.
/// </summary>
public enum ErrorType
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
    DomainError
}
