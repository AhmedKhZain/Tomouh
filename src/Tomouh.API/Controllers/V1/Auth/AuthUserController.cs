using Asp.Versioning;
using Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tomouh.API.Filters;
using Tomouh.Application.Auth.Queries.CheckEmailExistence;
using Tomouh.Contracts.Auth;

namespace Tomouh.API.Controllers.V1.Auth;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class AuthUserController : ApiControllerBase
{
    private readonly ISender _sender;

    public AuthUserController(CurrentUser? currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Checks if an email address already exists in the system. 
    /// Used for optimistic loading / instant validation during registration and login screens.
    /// </summary>
    /// <remarks>
    /// This is a GET query request, hence naturally idempotent by HTTP standard. No custom idempotency header required.
    /// </remarks>
    [HttpGet("check-email")]
    public async Task<IActionResult> CheckEmail(
        [FromQuery] string email,
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        CancellationToken cancellationToken = default
        )
    {
        var query = new CheckEmailExistenceQuery(email, idempotencyKey);
        var result = await _sender.Send(query, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Registers a new standard user account with email and password credentials.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' to prevent double submission and duplicate database record creation.
    /// </remarks>
    [HttpPost("register")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> Register(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] RegisterUserRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Authenticates a standard user via email and password, returning access tokens.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** No. Pure authentication requests generate dynamic state tracking data tokens and do not modify persistent entity data states.
    /// </remarks>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginUserCommand command,
        CancellationToken cancellationToken = default
        )
    {
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Registers a new user account utilizing an authenticated external Google OAuth token identity source.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' to prevent duplicate aggregate roots from spawning if the user double-clicks.
    /// </remarks>
    [HttpPost("register/google")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> RegisterWithGoogle(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] RegisterWithGoogleCommand command,
        CancellationToken cancellationToken = default
        )
    {
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Authenticates an existing user profile or logs them in directly using an external Google OAuth identity verification token payload.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** No. Generates fresh cryptographic authorization session token objects upon each invocation request.
    /// </remarks>
    [HttpPost("login/google")]
    public async Task<IActionResult> LoginWithGoogle(
        [FromBody] LoginWithGoogleCommand command,
        CancellationToken cancellationToken = default
        )
    {
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Refreshes expired authentication access tokens utilizing a valid cryptographically signed refresh token string state reference.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** No. Refresh token rotation strategies explicitly revoke old keys and issue new ones dynamically per invocation execution.
    /// </remarks>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        CancellationToken cancellationToken = default
        )
    {
        var command = new RefreshTokenCommand();
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Finalizes the registration account path by validating the provided email token challenge sequence sent to the user inbox.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Submitting the exact same confirmation parameters multiple times safely maps to the same finalized verified outcome.
    /// </remarks>
    [HttpPost("confirm-email")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> ConfirmEmail(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ConfirmEmailRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new ConfirmEmailCommand(request.UserEmail, request.Token, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Requests a secure system password reset numeric challenge sequence token to be dispatched to a verified email address.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** No. Multiple successive requests dispatch multiple notification messages or emails across external system network providers.
    /// </remarks>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordCommand command,
        CancellationToken cancellationToken = default
        )
    {
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Updates and overwrites the active user identity credential records with a new password sequence utilizing a validated out-of-band token payload challenge.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Applying the same reset request multiple times safely shifts the database state to the exact same target password hash.
    /// </remarks>
    [HttpPost("reset-password")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> ResetPassword(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var commad = new ResetPasswordCommand(request.Token, request.NewPassword, idempotencyKey);
        var result = await _sender.Send(commad, cancellationToken);
        return MapResult(result);
    }
}