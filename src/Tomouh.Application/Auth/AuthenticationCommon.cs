using Common.Errors;

namespace Tomouh.Application.Auth;

public static class AuthenticationCommon
{
    public static readonly string AccessTokenCookieName = "access_token";
    public static readonly string RefreshTokenCookieName = "refresh_token";
    public static readonly string UserOptimisticLoadingCachePrefix = "UserOptimisticLoading:";
    public static readonly TimeSpan UserOptimisticLoadingCacheExpiration = TimeSpan.FromMinutes(10);

    public static class AuthenticationErrors
    {
        public static readonly Error InvalidCredentials = Error.Validation(
            code: "Authentication.InvalidCredentials",
            description: "Invalid credentials");

        public static readonly Error SomethingGoseWrongEnterEmailAgain = Error.Validation(
            code: "Authentication.SomethingGoseWrongEnterEmailAgain",
            description: "Something went wrong. Please enter your email again.");

        public static Error UserNotFound(string email) => Error.NotFound(
            code: "Authentication.UserNotFound",
            description: $"User with email '{email}' not found.");

        public static Error UserBlocked(string email) => Error.Failure(
            code: "Authentication.UserBlocked",
            description: $"User with email '{email}' is blocked.");

        public static Error UserNotActive(string email) => Error.Failure(
            code: "Authentication.UserNotActive",
            description: $"User with email '{email}' is not active.");
    }
}