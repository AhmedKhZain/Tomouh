using Common.BaseTypes;
using Microsoft.EntityFrameworkCore;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Domain.Auth;

namespace Tomouh.Infrastructure.Persistence.Sql;

public class TomouhDbContext : DbContext
{
    private readonly IDomainEventCollector _eventCollector;

    public TomouhDbContext(DbContextOptions options, IDomainEventCollector eventCollector)
        : base(options)
    {
        _eventCollector = eventCollector;
    }

    public DbSet<UserToken> UserTokens { get; set; } = null!;



    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.Entries<IAggregate>()
            .Select(e => e.Entity)
            .ToList()
            .ForEach(aggregate => _eventCollector.CollectEventsFromAggregate(aggregate));

        return await base.SaveChangesAsync(cancellationToken);
    }
}