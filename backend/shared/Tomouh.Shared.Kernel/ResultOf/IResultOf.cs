using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Shared.Kernel.ResultOf;

public interface IResultOf
{
    List<Error>? Errors { get; }
    bool IsFailure { get; }
    bool IsDone { get; }
}
public interface IResultOf<out TValue> : IResultOf
{
    TValue Value { get; }
}
