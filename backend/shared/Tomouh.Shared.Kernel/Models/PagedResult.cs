namespace Tomouh.Shared.Kernel.Models;

/// <summary>
/// Represents a generic paginated data response containing items and total record count.
/// </summary>
public record PagedResult<T>(IReadOnlyList<T> Items, long TotalCount);