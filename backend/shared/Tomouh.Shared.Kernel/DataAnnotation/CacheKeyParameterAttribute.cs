namespace Tomouh.Shared.Kernel.DataAnnotation;

/// <summary>
/// Decorates properties of an <see cref="ICacheableQuery"/> to be included as key parameters in the generated cache key.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class CacheKeyParameterAttribute : Attribute
{
    public string? KeyName { get; }

    public CacheKeyParameterAttribute(string? keyName = null)
    {
        KeyName = keyName;
    }
}
