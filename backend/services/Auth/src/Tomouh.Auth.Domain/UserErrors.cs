using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Auth.Domain;

public static class UserErrors
{
    public static readonly Error PermissionAlreadyExists = Error.Conflict(
        code: "User.PermissionAlreadyExists",
        description: "The user profile already has this permission.");

    public static readonly Error PermissionDoesNotExist = Error.Conflict(
        code: "User.PermissionDoesNotExist",
        description: "The specified permission does not exist on this profile.");

    public static readonly Error ProfileNotFound = Error.NotFound(
        code: "User.ProfileNotFound",
        description: "The profile for the specified role was not found.");

    public static readonly Error ProfileAlreadyExists = Error.Conflict(
        code: "User.ProfileAlreadyExists",
        description: "A profile with this role already exists for this user.");

    public static readonly Error EmptyPassword = Error.Conflict(
        code: "User.EmptyPassword",
        description: "Password cannot be empty or whitespace.");

    public static readonly Error EmailAlreadyConfirmed = Error.Conflict(
        code: "User.EmailAlreadyConfirmed",
        description: "Cannot confirm an already confirmed email.");

    public static readonly Error InvalidPassword = Error.Conflict(
        code: "User.InvalidPassword",
        description: "The provided password is incorrect.");

    public static readonly Error TFAStatusUnchanged = Error.Conflict(
        code: "User.TFAStatusUnchanged",
        description: "TFA status is already in the requested state.");

    public static readonly Error InvalidTFAStatus = Error.Conflict(
        code: "User.InvalidTFAStatus",
        description: "The TFA status operation is invalid based on current date state.");

    public static readonly Error InvalidTokenType = Error.Conflict(
        code: "User.InvalidTokenType",
        description: "The provided token type is invalid for this operation."
    );

    public static readonly Error AlreadyUsedToken = Error.Conflict(
        code: "User.AlreadyUsedToken",
        description: "The provided token is already used."
    );

    public static readonly Error InvalidToken = Error.Conflict(
        code: "User.InvalidToken",
        description: "The provided token is invalid."
    );

    public static readonly Error ExpiredToken = Error.Conflict(
        code: "User.ExpiredToken",
        description: "The provided token has expired."
    );
    public static readonly Error RevokedToken = Error.Conflict(
        code: "User.RevokedToken",
        description: "The provided token has been revoked."
    );
    public static readonly Error ExternalLoginAlreadyExists = Error.Conflict(
        code: "User.ExternalLoginAlreadyExists",
        description: "An external login with this provider already exists for this user.");

    public static readonly Error ExternalLoginNotFound = Error.NotFound(
        code: "User.ExternalLoginNotFound",
        description: "The specified external login provider was not found for this user.");

    public static readonly Error CannotActivateBlockedAccount = Error.Conflict(
        code: "User.CannotActivateBlockedAccount",
        description: "Cannot activate an account that is currently blocked.");

    public static readonly Error CannotRemoveLastLoginMethod = Error.Conflict(
        code: "User.CannotRemoveLastLoginMethod",
        description: "Cannot remove the last login method (password or external login) from the user account.");

    public static readonly Error MetadataNotFound = Error.NotFound(
        code: "User.MetadataNotFound",
        description: "The specified metadata entry was not found.");
}