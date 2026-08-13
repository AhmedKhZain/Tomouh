using MediatR;
using System.Reflection;
using Tomouh.Shared.Kernel.DataAnnotation;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.ResultOf;
using Tomouh.Shared.Kernel.ResultOf.Errors;

namespace Tomouh.Shared.Kernel.CommonBehaviors;

/// <summary>
/// Pipeline behavior responsible for intercepting requests decorated with <see cref="AuthorizeAttribute"/>
/// and validating static Authentication (RBAC/ABAC) constraints.
/// </summary>
public class AuthorizationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IResultOf
{
    private readonly CurrentUser _currentUser;

    public AuthorizationBehavior(CurrentUser currentUser)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var authAttributes = typeof(TRequest)
            .GetCustomAttributes<AuthorizeAttribute>(true)
            .ToList();

        // Fast path: If public endpoint, proceed immediately
        if (!authAttributes.Any())
        {
            return await next(cancellationToken);
        }

        // 1. Check Authentication
        if (!_currentUser.IsAuthenticated)
        {
            return (dynamic)Error.Unauthorized("User is not authenticated.");
        }

        // 2. Validate Static Authorize Attributes
        foreach (var attr in authAttributes)
        {
            if (!ValidateStaticAuthorization(attr))
            {
                return (dynamic)Error.Forbidden("User is not authorized to execute this request.");
            }
        }

        return await next(cancellationToken);
    }

    private bool ValidateStaticAuthorization(AuthorizeAttribute attribute)
    {
        var requiredRoles = attribute.Roles?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        var requiredPermissions = attribute.Permissions?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            ?? Array.Empty<string>();

        // If attribute exists without roles/permissions specified, require authentication only
        if (requiredRoles.Length == 0 && requiredPermissions.Length == 0)
        {
            return true;
        }

        bool hasRoleAccess = HasRolesAccess(requiredRoles, attribute.RequireAllRoles);
        bool hasPermissionAccess = HasPermissionsAccess(requiredPermissions, attribute.RequireAllPermissions);

        if (attribute.RequireAnding)
        {
            return hasRoleAccess && hasPermissionAccess;
        }

        return hasRoleAccess || hasPermissionAccess;
    }

    private bool HasRolesAccess(string[] requiredRoles, bool requireAll)
    {
        if (requiredRoles.Length == 0) return true;

        return requireAll
            ? requiredRoles.All(r => _currentUser.IsInRole(r))
            : requiredRoles.Any(r => _currentUser.IsInRole(r));
    }

    private bool HasPermissionsAccess(string[] requiredPermissions, bool requireAll)
    {
        if (requiredPermissions.Length == 0) return true;

        return requireAll
            ? _currentUser.HasAllPermissions(requiredPermissions)
            : _currentUser.HasAnyPermission(requiredPermissions);
    }
}