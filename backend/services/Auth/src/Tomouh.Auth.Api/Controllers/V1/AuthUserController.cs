using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tomouh.Auth.Api.Filters;
using Tomouh.Auth.Application.Commands.ResetPassword;
using Tomouh.Auth.Application.Queries.CheckEmailExistence;
using Tomouh.Auth.Application.Queries.Login;
using Tomouh.Auth.Application.Queries.RefreshToken;
using Tomouh.Auth.Contracts;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Api.Controllers.V1;

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
    [RequireIdempotencyHeader]
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
        [FromBody] LoginQuery query,
        CancellationToken cancellationToken = default
        )
    {
        var result = await _sender.Send(query, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Registers a new user account utilizing an authenticated external OAuth provider token identity source.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' to prevent duplicate aggregate roots from spawning if the user double-clicks.
    /// </remarks>
    [HttpPost("register/external")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> RegisterWithExternalProvider(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ExternalAuthRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToRegisterCommand(idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Authenticates an existing user profile or logs them in directly using an external OAuth identity verification token payload.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Generates fresh cryptographic authorization session token objects upon each invocation request.
    /// </remarks>
    [HttpPost("login/external")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> LoginWithExternalProvider(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ExternalAuthRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var query = request.ToLoginQuery(idempotencyKey);
        var result = await _sender.Send(query, cancellationToken);
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
        var command = new RefreshTokenQuery();
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
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(idempotencyKey);
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