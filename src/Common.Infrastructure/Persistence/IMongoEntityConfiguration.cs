using Common.BaseTypes;
using MongoDB.Bson.Serialization;
using System.Reflection;

namespace Common.Infrastructure.Persistence;

public interface IMongoEntityConfiguration
{
    void Configure();
}

/// <summary>
/// Base MongoDB class map configuration for core entities.
/// Automatically configures primary keys, memory-only properties, and backing private fields.
/// </summary>
public abstract class BaseMongoEntityConfiguration<TEntity, TId> : IMongoEntityConfiguration
    where TEntity : class, IEntity<TId>
{
    public void Configure()
    {
        if (BsonClassMap.IsClassMapRegistered(typeof(TEntity)))
            return;

        BsonClassMap.RegisterClassMap<TEntity>(cm =>
        {
            cm.AutoMap();

            ConfigureId(cm);
            ConfigureIgnoredProperties(cm);
            ConfigurePrivateFields(cm);
            ConfigureEntity(cm);
        });
    }

    /// <summary>
    /// Override in derived classes to add specific document mapping logic.
    /// </summary>
    protected abstract void ConfigureEntity(BsonClassMap<TEntity> cm);

    /// <summary>
    /// Standardizes ID mapping for MongoDB.
    /// </summary>
    protected virtual void ConfigureId(BsonClassMap<TEntity> cm)
    {
        var idMember = cm.GetMemberMap(x => x.Id);
        idMember?.SetElementName("_id");
    }

    /// <summary>
    /// Automatically ignores common in-memory state properties.
    /// </summary>
    protected virtual void ConfigureIgnoredProperties(BsonClassMap<TEntity> cm)
    {
        var propertiesToIgnore = new[] { "IsUpdated", "DomainEvents" };

        foreach (var propName in propertiesToIgnore)
        {
            var prop = typeof(TEntity).GetProperty(
                propName,
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            if (prop != null && cm.GetMemberMap(propName) != null)
            {
                cm.UnmapProperty(propName);
            }
        }
    }

    /// <summary>
    /// Automatically maps private fields starting with underscore (e.g., _passwordHash -> passwordHash).
    /// </summary>
    protected virtual void ConfigurePrivateFields(BsonClassMap<TEntity> cm)
    {
        var fields = typeof(TEntity).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .Where(f => f.Name.StartsWith("_") && !f.Name.Contains("k__BackingField"));

        foreach (var field in fields)
        {
            var elementName = char.ToLowerInvariant(field.Name[1]) + field.Name[2..];

            if (cm.GetMemberMap(field.Name) == null)
            {
                cm.MapField(field.Name).SetElementName(elementName);
            }
        }
    }
}

/// <summary>
/// Specialized MongoDB configuration for Auditable Entities.
/// Handles naming conventions for auditing metadata fields.
/// </summary>
public abstract class AuditableMongoEntityConfiguration<TEntity, TId> : BaseMongoEntityConfiguration<TEntity, TId>
    where TEntity : class, IAuditable<TId>
{
    protected override void ConfigureIgnoredProperties(BsonClassMap<TEntity> cm)
    {
        base.ConfigureIgnoredProperties(cm);
        cm.UnmapProperty(a => a.IsUpdated);
    }


}