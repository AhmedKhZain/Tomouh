namespace Tomouh.Shared.Kernel.DataAnnotation;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class AuthorizeAttribute : Attribute
{
    public string? Roles { get; }
    public string? Permissions { get; }
    public bool RequireAllRoles { get; set; } = false;
    public bool RequireAllPermissions { get; set; } = false;
    public bool RequireAnding { get; set; } = false;

    public AuthorizeAttribute(
        string? roles = null,
        string? permissions = null,
        bool requireAllRoles = false,
        bool requireAllPermissions = false,
        bool requireAnding = false)
    {
        Roles = roles;
        Permissions = permissions;
        RequireAllRoles = requireAllRoles;
        RequireAllPermissions = requireAllPermissions;
        RequireAnding = requireAnding;
    }
}
