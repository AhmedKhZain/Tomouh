using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Tomouh.Shared.Kernel.BaseTypes;
using Tomouh.Shared.Kernel.Features;

namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts;

public class DbBaseContext : DbContext, IUnitOfWork
{
    private IDbContextTransaction? _dbTransaction;
    private bool _isInTransaction = false;

    public DbBaseContext(DbContextOptions options) : base(options)
    {
    }
    public bool IsInTransaction => _isInTransaction;


    public bool AnyEvents()
    {
        return ChangeTracker.Entries<IAggregate>().Any(e => e.Entity.DomainEvents.Any() || e.Entity.IntegrationEvents.Any());
    }

    public IReadOnlyList<IDomainEvent> CollectDomainEvents()
    {
        var events = new List<IDomainEvent>();
        ChangeTracker.Entries<IAggregate>().ToList().ForEach(e => events.AddRange(e.Entity.DequeueDomainEvents()));
        return events;
    }

    public IReadOnlyList<IIntegrationEvent> CollectIntegrationEvents()
    {
        var events = new List<IIntegrationEvent>();
        ChangeTracker.Entries<IAggregate>().ToList().ForEach(e =>
        {
            events.AddRange(e.Entity.DequeueIntegrationEvents());
        });

        return events;
    }

    public async Task StartTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbTransaction == null)
        {
            _dbTransaction = await Database.BeginTransactionAsync(cancellationToken);
            _isInTransaction = true;
        }
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await SaveChangesAsync(cancellationToken);

        if (_dbTransaction != null)
        {
            await _dbTransaction.CommitAsync(cancellationToken);
            await _dbTransaction.DisposeAsync();
            _dbTransaction = null;
            _isInTransaction = false;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_dbTransaction != null)
        {
            await _dbTransaction.RollbackAsync(cancellationToken);
            await _dbTransaction.DisposeAsync();
            _dbTransaction = null;
            _isInTransaction = false;
        }
    }


}
