using Common.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Tomouh.Application.Common.Models;

namespace Tomouh.Infrastructure.Persistence.Sql;

public class AppSystemDbContext : DbContext
{
    public AppSystemDbContext(DbContextOptions<AppSystemDbContext> options) : base(options)
    {

    }
    public DbSet<AuditLog> Audits { get; set; } = null!;
    public DbSet<EventOutbox> OutboxEvents { get; set; } = null!;



    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await base.SaveChangesAsync(cancellationToken);
    }

}
