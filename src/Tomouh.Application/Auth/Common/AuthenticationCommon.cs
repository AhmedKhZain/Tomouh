using Common.ResultOf.Errors;

namespace Tomouh.Application.Auth.Common;

public static class AuthenticationCommon
{
    public static readonly string AccessTokenCookieName = "access_token";
    public static readonly TimeSpan AccessTokenCookieExpiration = TimeSpan.FromHours(8);
    public static readonly string RefreshTokenCookieName = "refresh_token";
    public static readonly TimeSpan RefreshTokenCookieExpiration = TimeSpan.FromDays(8);
    public static readonly string UserOptimisticLoadingCachePrefix = "UserOptimisticLoading:";
    public static readonly TimeSpan UserOptimisticLoadingCacheExpiration = TimeSpan.FromMinutes(10);

    public static class AuthenticationErrors
    {
        public static readonly Error InvalidCredentials = Error.Validation(
            code: "Auth.InvalidCredentials",
            description: "Invalid credentials.");

        public static readonly Error SomethingGoesWrongEnterEmailAgain = Error.Validation(
            code: "Auth.SomethingGoesWrongEnterEmailAgain",
            description: "Something went wrong. Please enter your email again.");

        public static readonly Error UserShouldBeLoggedIn = Error.Validation(
            code: "Auth.UserShouldBeLoggedIn",
            description: "User must be logged in to perform this action.");

        public static readonly Error EmailConfirmationTokenNotFound = Error.NotFound(
            code: "Auth.EmailConfirmationTokenNotFound",
            description: "A valid email confirmation token was not found.");

        public static readonly Error UserNotFound = Error.NotFound(
            code: "Auth.UserNotFound",
            description: "User was not found.");

        public static readonly Error UserBlocked = Error.Unauthorized(
            code: "Auth.UserBlocked",
            description: "This user account has been blocked.");

        public static readonly Error UserIsDeactivated = Error.Failure(
            code: "Auth.UserDeactivated",
            description: "This user account is currently inactive.");

        public static readonly Error RefreshTokenMissing = Error.Unauthorized(
            code: "Auth.RefreshTokenMissing",
            description: "Refresh token is missing from the request cookies.");

        public static readonly Error InvalidRefreshToken = Error.Unauthorized(
            code: "Auth.InvalidRefreshToken",
            description: "The provided refresh token is invalid or has expired.");

        public static readonly Error InvalidResetToken = Error.Validation(
            code: "Auth.InvalidResetToken",
            description: "The provided password reset token is invalid or has expired.");

        public static readonly Error AccountAlreadyExists = Error.Conflict(
            code: "Auth.AccountAlreadyExists",
            description: "An account linked to this identity already exists. Please log in.");

        public static readonly Error EmailAlreadyRegisteredWithLocalAccount = Error.Conflict(
            code: "Auth.EmailAlreadyRegisteredWithLocalAccount",
            description: "This email is already registered using a password. Please log in using your email and password.");

        public static readonly Error InvalidGoogleToken = Error.Validation(
            code: "Auth.InvalidGoogleToken",
            description: "The provided Google authentication token is invalid or has expired.");

        public static readonly Error GoogleAuthFailed = Error.Failure(
            code: "Auth.GoogleAuthFailed",
            description: "Failed to authenticate with Google servers. Please try again later.");

        public static readonly Error GoogleAccountNotLinked = Error.NotFound(
            code: "Auth.GoogleAccountNotLinked",
            description: "No registered account was found linked to this Google identity.");
    }
}