namespace Common.ResultOf;

public record struct Done
{
    public string Message { get; init; } = "The action has been Done.";
    public Guid Id { get; init; }

    public Done(string message = null, Guid? id = null)
    {
        Message = message ?? "The action has been Done.";
        Id = id ?? Guid.Empty;
    }

    public static Done Default => new Done();
    public static Done Created => new Done(message: "The resource has been created.");
    public static Done Updated => new Done(message: "The resource has been updated.");

    public static Done NoContent(Guid? id = null, string message = null)
    {
        return id.HasValue
            ? new Done(message ?? $"The action has been Done with no content but Id = {id.Value}.", id.Value)
            : new Done(message ?? "The action has been Done with no content.");
    }
    public static Done NoContent(string message = null, Guid? id = null)
    {
        return id.HasValue
            ? new Done(message ?? $"The action has been Done with no content but Id = {id.Value}.", id.Value)
            : new Done(message ?? "The action has been Done with no content.");
    }
}