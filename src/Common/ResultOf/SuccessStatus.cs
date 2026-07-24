namespace Common.ResultOf;

[Flags]
public enum DoneStatus
{
    None = 0,
    Done = 1,
    NoContent = 2,
    Created = 4,
    Accepted = 8,
    Partial = 16,
    ZeroCount = 32,
    PartialZeroCount = 48,
    Cached = 64,
    CachedPartial = 80,
}
