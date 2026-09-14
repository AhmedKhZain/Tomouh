using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tomouh.Auth.Api.Filters;
using Tomouh.Auth.Application.Commands.ChangePassword;
using Tomouh.Auth.Application.Commands.ChangeTFAStatus;
using Tomouh.Auth.Contracts.Requests;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UserSecurityController : ApiControllerBase
{
    private readonly ISender _sender;

    public UserSecurityController(CurrentUser? currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Changes the Two-Factor Authentication (2FA) enablement status for the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate toggle attempts.
    /// </remarks>
    [HttpPut("tfa/status")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> ChangeTFAStatus(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ChangeTFAStatusRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new ChangeTFAStatusCommand(
            request.IsEnabled, request.Password, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Changes the currently authenticated user's password after verifying the current password.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate password changes.
    /// </remarks>
    [HttpPost("change-password")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> ChangePassword(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new ChangePasswordCommand(
            request.CurrentPassword, request.NewPassword, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }
}
