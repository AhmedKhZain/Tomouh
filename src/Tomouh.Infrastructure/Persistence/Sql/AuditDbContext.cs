using Common.AuditLogs;
using Microsoft.EntityFrameworkCore;

namespace Tomouh.Infrastructure.Persistence.Sql;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {

    }
    public DbSet<AuditLog> Audits { get; set; } = null!;
}
