using Common.BaseTypes;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tomouh.Infrastructure.Persistence.Sql;

public class AuditableEntityConfiguration<TEntity, TId> : BaseEntityConfiguration<TEntity, TId>
    where TEntity : class, IAuditable<TId>
{
    protected override void ConfigureEntity(EntityTypeBuilder<TEntity> builder)
    {
        base.ConfigureEntity(builder);

        builder.Property(a => a.LastUpdate).IsRequired(false);
        builder.Ignore(a => a.IsUpdated);
    }
}