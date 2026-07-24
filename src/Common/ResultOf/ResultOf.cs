using Common.Errors;

namespace Common.ResultOf;


public class ResultOf<TValue> : IResultOf<TValue>
{
    private TValue? _value { get; }
    private DoneStatus _status { get; }
    private List<Error>? _errors { get; } = new List<Error>();
    public bool IsDone => _errors.Count() == 0;
    public bool IsFailure => !IsDone;

    public List<Error>? Errors
    {
        get
        {
            if (IsDone)
            {
                return null;
            }
            return _errors;
        }
    }

    public TValue Value
    {
        get
        {
            if (IsFailure)
            {
                throw new NullReferenceException("Cannot access the value of a failed result.");
            }
            return _value!;
        }
    }


    private ResultOf(Error error)
    {
        _errors = [error];
    }

    private ResultOf(List<Error> errors)
    {
        _errors = errors;
    }

    private ResultOf(TValue value, DoneStatus status)
    {
        _value = value;
        _status = status;
    }

    public static ResultOf<TValue> FromValue(TValue value, DoneStatus status = DoneStatus.Done)
    {
        return new ResultOf<TValue>(value, status);
    }

    public static ResultOf<TValue> FromError(Error error)
    {
        return new ResultOf<TValue>(error);
    }

    public static ResultOf<TValue> FromErrors(List<Error> error)
    {
        return new ResultOf<TValue>(error);
    }

    public static implicit operator ResultOf<TValue>(TValue value)
    {
        return new ResultOf<TValue>(value, DoneStatus.Done);
    }

    public static implicit operator ResultOf<TValue>(Error error)
    {
        return new ResultOf<TValue>(error);
    }

    public static implicit operator ResultOf<TValue>(List<Error> error)
    {
        return new ResultOf<TValue>(error);
    }

    public TNextValue Match<TNextValue>(Func<TValue, DoneStatus, TNextValue> onValue, Func<List<Error>, TNextValue> onError)
    {
        if (IsFailure)
        {
            return onError(_errors);
        }

        return onValue(_value, _status);
    }
    public TNextValue Match<TNextValue>(Func<TValue, TNextValue> onValue, Func<List<Error>, TNextValue> onError)
    {
        if (IsFailure)
        {
            return onError(_errors);
        }

        return onValue(_value);
    }

    public static IResultOf<TValue> Success(object cachedValue)
    {
        return new ResultOf<TValue>((TValue)cachedValue, DoneStatus.Done);
    }
    public static IResultOf<TValue> Cached(TValue cachedValue)
    {
        return new ResultOf<TValue>((TValue)cachedValue, DoneStatus.Cached);
    }
}
