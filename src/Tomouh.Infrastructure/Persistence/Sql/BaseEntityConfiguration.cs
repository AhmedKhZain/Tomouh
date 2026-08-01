using Common.BaseTypes;
using Common.DataConvrters;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace Tomouh.Infrastructure.Persistence.Sql;

/// <summary>
/// Base class for entity configurations. Provides automatic mapping for common properties, 
/// value objects, and naming conventions.
/// </summary>
/// <typeparam name="TEntity">The entity type.</typeparam>
/// <typeparam name="TId">The entity identifier type.</typeparam>
public abstract class BaseEntityConfiguration<TEntity, TId> : IEntityTypeConfiguration<TEntity>
        where TEntity : class, IEntity<TId>
{
    // Configuration Constants
    // Configuration Constants

    /// <summary>
    /// Gets the maximum length for properties containing "Name". Default is 200.
    /// </summary>
    protected virtual int NameLength => 200;

    /// <summary>
    /// Gets the maximum length for description or note properties. Default is 1200.
    /// </summary>
    protected virtual int DescriptionLength => 1200;

    /// <summary>
    /// Gets the default maximum length for string properties that do not match other conventions. Default is 400.
    /// </summary>
    protected virtual int DefaultStringLength => 400;

    /// <summary>
    /// Gets the maximum length for email address properties. Default is 320.
    /// </summary>
    protected virtual int EmailLength => 320;

    /// <summary>
    /// Gets the maximum length for URL, URI, or path properties. Default is 1024.
    /// </summary>
    protected virtual int UrlPathLength => 1024;

    /// <summary>
    /// Gets the precision for decimal properties. Default is 18.
    /// </summary>
    protected virtual int DecimalPrecision => 18;

    /// <summary>
    /// Gets the scale for decimal properties. Default is 2.
    /// </summary>
    protected virtual int DecimalScale => 2;

    /// <summary>
    /// Gets a value indicating whether the ID should be generated on add. Default is true.
    /// </summary>
    protected virtual bool GenerateIdOnAdd => true;

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        ConfigureId(builder);
        ConfigureStringLengths(builder);
        ConfigureDecimalPrecision(builder);
        ConfigureOwnedTypes(builder);
        ConfigureEnums(builder);
        ConfigureEntity(builder);
    }

    /// <summary>
    /// Configures the entity's primary key. Override to handle complex keys.
    /// </summary>
    protected virtual void ConfigureId(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(x => x.Id);

        if (typeof(TId) == typeof(Guid))
        {
            if (GenerateIdOnAdd)
                builder.Property(x => x.Id).ValueGeneratedOnAdd();
            else
                builder.Property(x => x.Id).ValueGeneratedNever();
        }
        else
        {
            ConfigureIdOverride(builder);
        }
    }

    /// <summary>
    /// Override this method in derived classes to provide custom configuration for non-Guid identifiers.
    /// </summary>
    protected virtual void ConfigureIdOverride(EntityTypeBuilder<TEntity> builder) { }

    /// <summary>
    /// Override this method to add entity-specific configurations (relationships, indexes, etc.).
    /// </summary>
    protected virtual void ConfigureEntity(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(e => !e.IsDeleted);
    }

    /// <summary>
    /// Automatically applies MaxLength conventions to string properties based on naming.
    /// </summary>
    private void ConfigureStringLengths(EntityTypeBuilder<TEntity> builder)
    {
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)
            .Where(p => p.PropertyType == typeof(string));

        foreach (var prop in properties)
        {
            var pb = builder.Property(prop.Name);
            ApplyStringNamingConvention(pb, prop.Name);
        }
    }

    /// <summary>
    /// Automatically applies Precision and Scale to decimal properties.
    /// </summary>
    private void ConfigureDecimalPrecision(EntityTypeBuilder<TEntity> builder)
    {
        var decimalProperties = typeof(TEntity)
            .GetProperties()
            .Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));

        foreach (var prop in decimalProperties)
        {
            builder.Property(prop.Name).HasPrecision(DecimalPrecision, DecimalScale);
        }
    }

    /// <summary>
    /// Automatically detects and configures Owned Types (Value Objects).
    /// </summary>
    private void ConfigureOwnedTypes(EntityTypeBuilder<TEntity> builder)
    {
        var properties = typeof(TEntity).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => IsOwnedType(p.PropertyType));

        foreach (var prop in properties)
        {
            var navigation = builder.OwnsOne(prop.PropertyType, prop.Name);
            ConfigureOwnedStrings(navigation, prop.PropertyType);
            ConfigureOwnedDecimals(navigation, prop.PropertyType);
        }
    }
    /// <summary>
    /// Scans for Enums with [StoreEnumAsString] attribute and applies conversion automatically.
    /// </summary>
    protected virtual void ConfigureEnums(EntityTypeBuilder<TEntity> builder)
    {
        var properties = typeof(TEntity).GetProperties()
            .Where(p => p.PropertyType.IsEnum || (Nullable.GetUnderlyingType(p.PropertyType)?.IsEnum ?? false));

        foreach (var prop in properties)
        {
            var attribute = prop.GetCustomAttribute<StoreEnumAsStringAttribute>()
                          ?? prop.PropertyType.GetCustomAttribute<StoreEnumAsStringAttribute>()
                          ?? Nullable.GetUnderlyingType(prop.PropertyType)?.GetCustomAttribute<StoreEnumAsStringAttribute>();

            if (attribute != null)
            {
                var pb = builder.Property(prop.Name);
                var converter = CreateEnumConverter(prop.PropertyType, attribute);

                pb.HasConversion(converter)
                  .HasMaxLength(attribute.MaxLength);
            }
        }
    }

    private ValueConverter CreateEnumConverter(Type propertyType, StoreEnumAsStringAttribute attr)
    {
        var enumType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;

        var toString = new Func<object?, string?>(value =>
        {
            if (value == null) return null;

            var str = value.ToString()!;
            return attr.NamingStrategy switch
            {
                EnumNamingStrategy.Uppercase => str.ToUpper(),
                EnumNamingStrategy.Lowercase => str.ToLower(),
                _ => str
            };
        });

        var fromString = new Func<string?, object?>(value =>
        {
            if (string.IsNullOrWhiteSpace(value)) return null;

            return Enum.Parse(enumType, value, attr.CaseInsensitive);
        });

        var converterType = typeof(ValueConverter<,>).MakeGenericType(propertyType, typeof(string));

        return (ValueConverter)Activator.CreateInstance(converterType, toString, fromString, null)!;
    }

    private void ConfigureOwnedStrings(OwnedNavigationBuilder navigation, Type type)
    {
        var stringProps = type.GetProperties().Where(p => p.PropertyType == typeof(string));
        foreach (var p in stringProps)
        {
            var pb = navigation.Property<string>(p.Name);

            pb.Metadata.SetColumnName(p.Name);

            ApplyStringNamingConvention(pb, p.Name);
        }
    }


    private void ConfigureOwnedDecimals(OwnedNavigationBuilder navigation, Type type)
    {
        var decimalProps = type.GetProperties().Where(p => p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?));
        foreach (var p in decimalProps)
        {
            navigation.Property(p.Name).HasPrecision(DecimalPrecision, DecimalScale);
        }
    }

    private bool IsOwnedType(Type type)
    {
        return type.IsClass && type != typeof(string) && !type.Namespace.StartsWith("System");
    }

    private void ApplyStringNamingConvention(PropertyBuilder pb, string name)
    {
        var lower = name.ToLowerInvariant();
        if (lower.Contains("name") && !lower.Contains("username")) pb.HasMaxLength(NameLength);
        else if (lower.Contains("description") || lower.Contains("note")) pb.HasMaxLength(DescriptionLength);
        else if (lower.Contains("email")) pb.HasMaxLength(EmailLength);
        else if (lower.EndsWith("path") || lower.EndsWith("url")) pb.HasMaxLength(UrlPathLength);
        else pb.HasMaxLength(DefaultStringLength);
    }
}
