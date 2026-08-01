using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tomouh.Domain.Auth;

namespace Tomouh.Infrastructure.Persistence.Sql.Configurations;

public class UserTokenConfiguration : IEntityTypeConfiguration<UserToken>
{
    public void Configure(EntityTypeBuilder<UserToken> builder)
    {
        builder.ToTable("UserTokens");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.TokenHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(t => t.UserId)
            .IsRequired();

        // Configure TokenType SmartEnum/ValueObject conversion
        builder.Property(t => t.TokenType)
            .HasConversion(
                type => type.Name,
                name => TokenType.FromName(name, true))
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(t => t.IsUsed)
            .IsRequired();

        builder.Property(t => t.UsedAt)
            .IsRequired(false);

        builder.Property(t => t.IsRevoked)
            .IsRequired();

        // Configure TokenRevokeCause Enum conversion
        builder.Property(t => t.RevokeCause)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(t => t.RevokedAt)
            .IsRequired(false);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        // Ignore computed property
        builder.Ignore(t => t.IsExpired);

        //// Foreign Key Relationship
        //builder.HasOne(t => t.User)
        //    .WithMany()
        //    .HasForeignKey(t => t.UserId)
        //    .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(t => t.TokenHash);
        builder.HasIndex(t => new { t.UserId, t.TokenType });
        builder.HasIndex(t => new { t.TokenHash, t.TokenType, t.UserId });
    }
}