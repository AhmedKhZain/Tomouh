using System.Collections.Concurrent;
using System.Reflection;

namespace Common.BaseTypes;

[AttributeUsage(AttributeTargets.Property)]
public class EqualityComponentAttribute : Attribute { }

public abstract class ValueObject : IEquatable<ValueObject>
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propertiesCache = new();

    private PropertyInfo[] GetProperties()
    {
        return _propertiesCache.GetOrAdd(GetType(), type =>
            type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.IsDefined(typeof(EqualityComponentAttribute), true))
                .ToArray()
        );
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

    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
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

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (left is null && right is null) return true;
        if (left is null || right is null) return false;
        return left.Equals(right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}