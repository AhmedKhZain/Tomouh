using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Tomouh.Auth.Api.Filters;
using Tomouh.Auth.Application.Commands.AddExternalLogin;
using Tomouh.Auth.Application.Commands.AddOrUpdateProfileMetadata;
using Tomouh.Auth.Application.Commands.AddProfile;
using Tomouh.Auth.Application.Commands.ConfirmEmail;
using Tomouh.Auth.Application.Commands.RemoveExternalLogin;
using Tomouh.Auth.Application.Commands.RemoveProfileMetadata;
using Tomouh.Auth.Application.Commands.UpdateUserData;
using Tomouh.Auth.Contracts.Requests;
using Tomouh.Auth.Domain.Enums;
using Tomouh.Shared.Kernel.Models;

namespace Tomouh.Auth.Api.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class UserDataController : ApiControllerBase
{
    private readonly ISender _sender;

    public UserDataController(CurrentUser? currentUser, ISender sender) : base(currentUser)
    {
        _sender = sender;
    }

    /// <summary>
    /// Links a new external OAuth provider account to the currently authenticated user profile.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate link attempts.
    /// </remarks>
    [HttpPost("external-logins")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> AddExternalLogin(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] AddExternalLoginRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new AddExternalLoginCommand(
            request.Provider, request.Token, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Unlinks an external OAuth provider account from the currently authenticated user profile.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Unlinking an already missing provider yields the same result state.
    /// </remarks>
    [HttpDelete("external-logins/{provider}")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> RemoveExternalLogin(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] AuthProvider provider,
        CancellationToken cancellationToken = default
        )
    {
        var command = new RemoveExternalLoginCommand(provider, idempotencyKey);
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
        var command = new ConfirmEmailCommand(request.Token, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Updates the core basic data of the currently authenticated user (show name, names, email).
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate update attempts.
    /// </remarks>
    [HttpPut("profile")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> UpdateProfile(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] UpdateUserDataRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new UpdateUserDataCommand(
            request.ShowName, request.FirstName, request.LastName,
            request.Email, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Adds a new role profile to the currently authenticated user account.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate profile creation.
    /// </remarks>
    [HttpPost("profiles")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> AddProfile(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromBody] AddProfileRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new AddProfileCommand(request.Role, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Adds or updates a metadata key-value pair on a specific role profile of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Requires 'X-Idempotency-Key' header to prevent duplicate metadata operations.
    /// </remarks>
    [HttpPut("profiles/{role}/metadata")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> AddOrUpdateProfileMetadata(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Role role,
        [FromBody] AddOrUpdateProfileMetadataRequest request,
        CancellationToken cancellationToken = default
        )
    {
        var command = new AddOrUpdateProfileMetadataCommand(
            role, request.Key, request.Value, request.IsPublic, request.MetadataType, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }

    /// <summary>
    /// Removes a metadata key from a specific role profile of the currently authenticated user.
    /// </summary>
    /// <remarks>
    /// **Idempotent:** Yes. Removing an already missing key yields the same result state.
    /// </remarks>
    [HttpDelete("profiles/{role}/metadata/{key}")]
    [RequireIdempotencyHeader]
    public async Task<IActionResult> RemoveProfileMetadata(
        [FromHeader(Name = "X-Idempotency-Key")] Guid idempotencyKey,
        [FromRoute] Role role,
        [FromRoute] string key,
        CancellationToken cancellationToken = default
        )
    {
        var command = new RemoveProfileMetadataCommand(role, key, idempotencyKey);
        var result = await _sender.Send(command, cancellationToken);
        return MapResult(result);
    }
}
