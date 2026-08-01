using Common.BaseTypes;
using Microsoft.EntityFrameworkCore;
using Tomouh.Application.Common.Interfaces;
using Tomouh.Domain.Auth;
using Tomouh.Domain.Funders;
using Tomouh.Domain.Scholarships;

namespace Tomouh.Infrastructure.Persistence.Sql;

public class TomouhDbContext
    : DbContext
{
    private readonly IDomainEventCollector _eventCollector;

    public TomouhDbContext(DbContextOptions<TomouhDbContext> options, IDomainEventCollector eventCollector)
        : base(options)
    {
        _eventCollector = eventCollector;
    }


    public DbSet<Scholarship> Scholarships { get; set; } = null!;
    public DbSet<Funder> FundOrganizations { get; set; } = null!;
    public DbSet<UserToken> UserTokens { get; set; } = null!;


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TomouhDbContext).Assembly);

    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ChangeTracker.Entries<IAggregate>()
            .Select(e => e.Entity)
            .ToList()
            .ForEach(aggregate => _eventCollector.CollectEventsFromAggregate(aggregate));
        return await base.SaveChangesAsync(cancellationToken);
    }


}