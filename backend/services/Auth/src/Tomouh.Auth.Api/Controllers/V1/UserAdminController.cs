using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tomouh.Auth.Api.Filters;
using Tomouh.Auth.Application.Commands.GrantPermissionToProfile;
using Tomouh.Auth.Application.Commands.RevokePermissionFromProfile;
using Tomouh.Auth.Application.Commands.SetAccountActivationStatus;
using Tomouh.Auth.Application.Commands.SetBlockStatus;
using Tomouh.Auth.Application.Commands.SetCommentingStatus;
using Tomouh.Auth.Contracts;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UserAdminController : ApiControllerBase
{
    private readonly ISender _sender;

    public UserAdminController(CurrentUser? currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Activates or deactivates the target user account.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate status updates.
    /// **Authorization:** Requires the 'Admin' role AND the 'Users.ManageStatus' permission.
    /// </remarks>
    [HttpPut("{userId}/activation-status")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> SetAccountActivationStatus(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Guid userId,
        [FromBody] SetAccountActivationStatusRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(userId, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Enables or disables the target user's ability to post comments.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate status updates.
    /// **Authorization:** Requires the 'Users.ManageCommenting' permission.
    /// </remarks>
    [HttpPut("{userId}/commenting-status")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> SetCommentingStatus(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Guid userId,
        [FromBody] SetCommentingStatusRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(userId, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Blocks or unblocks the target user account, managing its activation state as well.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate block operations.
    /// **Authorization:** Requires the 'Admin' role AND the 'Users.Block' permission.
    /// </remarks>
    [HttpPut("{userId}/block-status")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> SetBlockStatus(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Guid userId,
        [FromBody] SetBlockStatusRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(userId, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Grants a specific permission to a role profile of the target user.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate grant attempts.
    /// **Authorization:** Requires the 'Admin' role AND the 'Permissions.Grant' permission.
    /// </remarks>
    [HttpPost("{userId}/profiles/{role}/permissions")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> GrantPermissionToProfile(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Guid userId,
        [FromRoute] Role role,
        [FromBody] GrantPermissionToProfileRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = request.ToCommand(userId, role, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Revokes a specific permission from a role profile of the target user.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate revoke attempts.
    /// **Authorization:** Requires the 'Admin' role AND the 'Permissions.Revoke' permission.
    /// </remarks>
    [HttpDelete("{userId}/profiles/{role}/permissions/{permission}")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> RevokePermissionFromProfile(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Guid userId,
        [FromRoute] Role role,
        [FromRoute] string permission,
        CancellationToken cancellationToken = default
        )
    {
        var command = new RevokePermissionFromProfileCommand(userId, role, permission, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }
}
