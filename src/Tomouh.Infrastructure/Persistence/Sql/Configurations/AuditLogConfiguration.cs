using Common.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Tomouh.Infrastructure.Persistence.Sql.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.EntityName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.EntityId)
            .IsRequired()
            .HasMaxLength(120);

        // Configure AuditActionType conversion
        builder.Property(a => a.Action)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.OldValues)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(a => a.From)
            .IsRequired();

        builder.Property(a => a.To)
            .IsRequired();

        builder.Property(a => a.IsRecovered)
            .IsRequired();

        builder.Property(a => a.RecoveredAt)
            .IsRequired(false);

        builder.Property(a => a.RecoveredByUserId)
            .IsRequired(false);

        builder.Property(a => a.CreatedBy)
            .IsRequired(false);
        builder.Property(a => a.ActorType)
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        // Indexes for efficient querying & filtering
        builder.HasIndex(a => new { a.EntityName, a.EntityId });
        builder.HasIndex(a => a.CreatedBy);
        builder.HasIndex(a => a.To);
        builder.HasIndex(a => a.ActorType);
    }
}