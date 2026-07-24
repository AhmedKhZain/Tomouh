namespace Tomouh.API.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class RequireIdempotencyHeaderAttribute : Attribute { }