using Common.ResultOf.Errors;

namespace Common.ResultOf;

public interface IResultOf
{
    List<Error>? Errors { get; }
    bool IsFailure { get; }
}
public interface IResultOf<out TValue> : IResultOf
{
    TValue Value { get; }
}
