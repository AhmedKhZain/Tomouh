namespace Tomouh.Auth.Api.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class RequireIdempotencyHeaderAttribute : Attribute { }