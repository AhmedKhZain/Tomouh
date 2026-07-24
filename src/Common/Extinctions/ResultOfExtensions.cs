using Common.Errors;
using Common.ResultOf;

namespace Common.Extinctions;

public static class ResultOfExtensions
{
    public static ResultOf<TValue> ToResultOf<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value);
    }
    public static ResultOf<TValue> ToResultOf<TValue>(this Error error)
    {
        return ResultOf<TValue>.FromError(error);
    }
    public static ResultOf<TValue> ToResultOf<TValue>(this List<Error> errors)
    {
        return ResultOf<TValue>.FromErrors(errors);
    }
    public static ResultOf<TValue> ToResultOf<TValue>(this IEnumerable<Error> errors)
    {
        return ResultOf<TValue>.FromErrors(errors.ToList());
    }


    public static ResultOf<TValue> AsDone<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.Done);
    }

    public static ResultOf<TValue> AsCreated<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.Created);
    }

    public static ResultOf<TValue> AsAccepted<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.Accepted);
    }

    public static ResultOf<TValue> AsPartial<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.Partial);
    }

    public static ResultOf<TValue> AsNoContent<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.NoContent);
    }
    public static ResultOf<TValue> AsZeroCount<TValue>(this TValue value)
    {
        return ResultOf<TValue>.FromValue(value, DoneStatus.PartialZeroCount);
    }
}
