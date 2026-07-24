
using Common.ResultOf;

namespace Common.Errors;

/// <summary>
/// Represents an error.
/// </summary>
public readonly record struct Error
{
    private Error(string code, string description, ErrorType type, Dictionary<string, object>? metadata)
    {
        Code = code;
        Description = description;
        Type = type;
        NumericType = (int)type;
        Metadata = metadata;
    }


    public string Code { get; }


    public string Description { get; }


    public ErrorType Type { get; }


    public int NumericType { get; }


    public Dictionary<string, object>? Metadata { get; }


    public static Error Failure(string code = "General.Failure", string description = "A failure has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.Failure, metadata);
    }

    public static Error Unexpected(string code = "General.Unexpected", string description = "An unexpected error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.Unexpected, metadata);
    }

    public static Error Validation(string code = "General.Validation", string description = "A validation error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.Validation, metadata);
    }

    public static Error Conflict(
        string code = "General.Conflict",
        string description = "A conflict error has occurred.",
        Dictionary<string, object>? metadata = null) =>
            new(code, description, ErrorType.Conflict, metadata);

    public static Error NotFound(string code = "General.NotFound", string description = "A 'Not Found' error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.NotFound, metadata);
    }

    public static Error Unauthorized(string code = "General.Unauthorized", string description = "An 'Unauthorized' error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.Unauthorized, metadata);
    }

    public static Error Forbidden(string code = "General.Forbidden", string description = "A 'Forbidden' error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.Forbidden, metadata);
    }
    public static Error DomainFailure(string code = "Domain.Failure", string description = "A 'Failure' error has occurred.", Dictionary<string, object>? metadata = null)
    {
        return new(code, description, ErrorType.DomainError, metadata);
    }

    public static Error Custom(
        int type,
        string code,
        string description,
        Dictionary<string, object>? metadata = null) =>
            new(code, description, (ErrorType)type, metadata);


    public ResultOf<T> AsError<T>()
    {
        return ResultOf<T>.FromError(this);
    }
    public IResultOf<T> AsAbstractError<T>()
    {
        return ResultOf<T>.FromError(this);
    }
}
