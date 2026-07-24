using Common.Errors;
using Common.Markups;
using Common.Models;
using Common.ResultOf;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace Tomouh.API.Controllers;

public class ApiControllerBase : ControllerBase
{
    protected readonly CurrentUser? _currentUser;
    private readonly DateTime _requestArrive;

    public ApiControllerBase(CurrentUser? currentUser)
    {
        _currentUser = currentUser;
        _requestArrive = DateTime.UtcNow;
    }

    protected IActionResult Success<T>(T value, DoneStatus status)
    {
        return status switch
        {
            DoneStatus.Created
                => StatusCode(201, CreateSuccessResponse(value, "Created successfully", 201)),

            DoneStatus.Done
                => Ok(CreateSuccessResponse(value, "Success", 200)),

            DoneStatus.Accepted
                => Accepted(CreateSuccessResponse(value, "Accepted", 202)),

            DoneStatus.PartialZeroCount
                => StatusCode(206, CreateSuccessResponse(value, "Zero object found", 206, 0, null)),

            DoneStatus.Partial when value is IEnumerable<ITotalCountIncluded> countIncludeds
                => StatusCode(206, CreateSuccessResponse(value, "Partial content", 206, countIncludeds.Count(), countIncludeds.FirstOrDefault()?.TotalCount)),

            DoneStatus.Partial when value is ITotalCountIncluded countIncluded
                => StatusCode(206, CreateSuccessResponse(value, "Partial content", 206, 1, countIncluded.TotalCount)),

            DoneStatus.Partial when value is IEnumerable enumerable
                => StatusCode(206, CreateSuccessResponse(value, "Partial content", 206, enumerable.Cast<object>().Count())),

            DoneStatus.NoContent
                => StatusCode(204),

            _ => Ok(CreateSuccessResponse(value, "Success", 200))
        };
    }

    protected IActionResult ErrorsPassed(List<Error> errors)
    {
        if (errors == null || errors.Count == 0)
        {
            return StatusCode(500, CreateErrorResponse(
                new List<Error> { Error.Unexpected("Unexpected.Error", "An unexpected error occurred.") },
                500,
                "An unexpected error occurred."));
        }

        var statusCode = errors[0].Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        string message = errors.Count > 1 ? "Multiple errors occurred." : errors[0].Description;

        return StatusCode(statusCode, CreateErrorResponse(errors, statusCode, message));
    }

    // دالة موحدة لبناء الـ Success Response
    private ApiResponse<T> CreateSuccessResponse<T>(
        T? data,
        string? message,
        int statusCode,
        int? count = null,
        int? totalCount = null)
    {
        var responseTime = DateTime.UtcNow;
        var duration = responseTime - _requestArrive;
        return new ApiResponse<T>(
            Data: data,
            Message: message,
            StatusCode: statusCode,
            ResponseTime: responseTime,
            ArriveAt: _requestArrive,
            ValueType: typeof(T).Name,
            Count: count,
            TotalCount: totalCount,
            UserId: _currentUser?.Id,
            IpAddress: _currentUser?.UserIP?.ToString(),
            DurationOfServiceInMilliseconds: duration.TotalMilliseconds
        );
    }

    private ApiResponse<object?> CreateErrorResponse(List<Error> errors, int statusCode, string message)
    {
        var responseTime = DateTime.UtcNow;
        var duration = responseTime - _requestArrive;

        var apiErrors = errors.Select(e => new ApiErrorResponse(e.Code, e.Description)).ToList();

        return new ApiResponse<object?>(
            Data: null,
            Message: message,
            StatusCode: statusCode,
            ResponseTime: responseTime,
            ArriveAt: _requestArrive,
            ValueType: null,
            Count: null,
            TotalCount: null,
            UserId: _currentUser?.Id,
            IpAddress: _currentUser?.UserIP?.ToString(),
            DurationOfServiceInMilliseconds: duration.TotalMilliseconds,
            Errors: apiErrors
        );
    }

    protected IActionResult MapResult<T>(ResultOf<T> result)
    {
        return result.Match(
            onValue: (value, status) => Success(value, status),
            onError: errors => ErrorsPassed(errors)
        );
    }
}
