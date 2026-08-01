namespace Common.Enums
{
    using System.Reflection;

    public abstract class SmartEnum<TEnum> : IEquatable<SmartEnum<TEnum>>
        where TEnum : SmartEnum<TEnum>
    {
        public string Name { get; }
        public int Value { get; }

        private static readonly Lazy<Dictionary<int, TEnum>> _fromValueCache;

        private static readonly Lazy<Dictionary<string, TEnum>> _fromNameIgnoreCaseCache;

        private static readonly Lazy<Dictionary<string, TEnum>> _fromNameCaseSensitiveCache;

        static SmartEnum()
        {
            var allItems = new Lazy<List<TEnum>>(LoadAll);

            _fromValueCache = new Lazy<Dictionary<int, TEnum>>(() =>
                allItems.Value.ToDictionary(item => item.Value));

            _fromNameIgnoreCaseCache = new Lazy<Dictionary<string, TEnum>>(() =>
                allItems.Value.ToDictionary(item => item.Name, StringComparer.OrdinalIgnoreCase));

            _fromNameCaseSensitiveCache = new Lazy<Dictionary<string, TEnum>>(() =>
                allItems.Value.ToDictionary(item => item.Name, StringComparer.Ordinal));
        }

        protected SmartEnum(string name, int value)
        {
            Name = name;
            Value = value;
        }

        private static List<TEnum> LoadAll()
        {
            return typeof(TEnum)
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(f => typeof(TEnum).IsAssignableFrom(f.FieldType))
                .Select(f => (TEnum)f.GetValue(null)!)
                .ToList();
        }

        public static IReadOnlyList<TEnum> GetAll() => _fromValueCache.Value.Values.ToList();

        public static TEnum? FromValue(int value)
            => _fromValueCache.Value.GetValueOrDefault(value);

        public static TEnum? FromName(string name, bool caseSensitive = false)
        {
            var cache = caseSensitive ? _fromNameCaseSensitiveCache.Value : _fromNameIgnoreCaseCache.Value;
            return cache.GetValueOrDefault(name);
        }

        public static bool TryFromValue(int value, out TEnum? result)
            => _fromValueCache.Value.TryGetValue(value, out result);

        public static bool TryFromName(string name, out TEnum? result, bool caseSensitive = false)
        {
            var cache = caseSensitive ? _fromNameCaseSensitiveCache.Value : _fromNameIgnoreCaseCache.Value;
            return cache.TryGetValue(name, out result);
        }

        public override string ToString() => Name;

        public override bool Equals(object? obj) => obj is SmartEnum<TEnum> other && Equals(other);
        public bool Equals(SmartEnum<TEnum>? other) => other is not null && Value.Equals(other.Value);
        public override int GetHashCode() => Value.GetHashCode();
        public static bool operator ==(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right)
            => ReferenceEquals(left, right) || (left is not null && left.Equals(right));
        public static bool operator !=(SmartEnum<TEnum>? left, SmartEnum<TEnum>? right) => !(left == right);
    }

}
