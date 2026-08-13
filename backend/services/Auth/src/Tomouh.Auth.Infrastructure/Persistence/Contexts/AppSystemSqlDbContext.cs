using Microsoft.EntityFrameworkCore;
using Tomouh.Auth.Domain.Entities;
using Tomouh.Shared.Kernel.Models;
using Tomouh.Shared.Kernel.Outbox;

namespace Tomouh.Auth.Infrastructure.Persistence.Contexts;

public class AppSystemSqlDbContext(
    DbContextOptions<AppSystemSqlDbContext> options,
    CurrentUser currentUser) : DbContext(options)
{
    public CurrentUser CurrentUser => currentUser;


    public DbSet<UserToken> UserTokens { get; set; }

    public DbSet<EventOutbox> Events { get; set; }
}
