namespace Common.Models;

public record ApiResponse<T>(
    T? Data,
    string? Message = null,
    int StatusCode = 200,
    DateTime ArriveAt = default,
    DateTime ResponseTime = default,
    string? ValueType = null,
    int? Count = null,
    int? TotalCount = null,
    Guid? UserId = null,
    string? IpAddress = null,
    double? DurationOfServiceInMilliseconds = null,
    List<ApiErrorResponse>? Errors = null
);

public record ApiErrorResponse(string Code, string Message);