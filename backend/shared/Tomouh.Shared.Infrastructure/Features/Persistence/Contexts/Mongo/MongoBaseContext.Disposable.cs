namespace Tomouh.Shared.Infrastructure.Features.Persistence.Contexts.Mongo;

public abstract partial class MongoBaseContext
{
    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs asynchronous application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        Dispose(disposing: false);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases unmanaged and optionally managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session?.Dispose();
                _session = null;
                TrackedEntities.Clear();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Asynchronously releases unmanaged and optionally managed resources.
    /// </summary>
    protected virtual ValueTask DisposeAsyncCore()
    {
        if (_session is not null)
        {
            _session.Dispose();
            _session = null;
        }

        TrackedEntities.Clear();
        return ValueTask.CompletedTask;
    }
}