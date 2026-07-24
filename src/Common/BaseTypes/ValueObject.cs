using System.Reflection;

namespace Common.BaseTypes;

[AttributeUsage(AttributeTargets.Property)]
public class EqualityComponentAttribute : Attribute { }

public abstract record ValueObject
{
    private static readonly Dictionary<Type, PropertyInfo[]> _propertiesCache = new();

    private PropertyInfo[] GetProperties()
    {
        var type = GetType();
        if (!_propertiesCache.TryGetValue(type, out var properties))
        {
            properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.IsDefined(typeof(EqualityComponentAttribute), true))
                .ToArray();
            _propertiesCache[type] = properties;
        }
        return properties;
    }

    private IEnumerable<object?> GetEqualityComponents()
    {
        return GetProperties().Select(p => p.GetValue(this));
    }

    public virtual bool Equals(ValueObject? other)
    {
        if (other is null || other.GetType() != GetType()) return false;

        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        foreach (var component in GetEqualityComponents())
        {
            hashCode.Add(component);
        }
        return hashCode.ToHashCode();
    }
}